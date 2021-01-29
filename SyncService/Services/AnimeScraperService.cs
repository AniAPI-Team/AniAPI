using Commons;
using Commons.Collections;
using Newtonsoft.Json;
using SyncService.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Commons.Enums;
using System.Threading.Tasks;
using PuppeteerSharp;
using SpotifyAPI.Web;

namespace SyncService.Services
{
    public class AnimeScraperService : IService
    {
        #region Members

        private AnimeCollection _animeCollection = new AnimeCollection();
        private HttpClient _anilistClient = new HttpClient() { BaseAddress = new Uri("https://graphql.anilist.co") };
        private GraphQLQuery _anilistQuery = new GraphQLQuery()
        {
            Query = @"
            query($page: Int, $format: MediaFormat) {
                Page(page: $page, perPage: 50) {
                    pageInfo {
                        lastPage
                    }
                    media(format: $format) {
                        id
                        idMal
                        format
                        status
                        title {
                            romaji
                            native
                        }
                        description
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        season
                        seasonYear
                        episodes
                        duration
                        trailer {
                            id
                            site
                        }
                        coverImage {
                            large
                            color
                        }
                        bannerImage
                        genres
                        relations {
                            edges {
                                relationType
                                node {
                                    id
                                    format
                                }
                            }
                        }
                        nextAiringEpisode {
                            episode
                        }
                        averageScore
                    }
                }
            }
            ",
            Variables = new Dictionary<string, object>()
        };
        private Browser _puppeteerClient;
        private SpotifyClient _spotifyClient;
        private int _totalPages = 1;
        private List<string> _formatsFilter = new List<string>() { "TV", "TV_SHORT", "MOVIE", "SPECIAL", "OVA", "ONA", "MUSIC" };
        private int _rateLimitRemaining;
        private long _rateLimitReset;

        protected override int TimeToWait => 60 * 1000 * 60 * 12; // 12 Hours

        #endregion
        
        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus()
            {
                Name = "AnimeScraper",
                Status = ServiceStatusEnum.NONE
            };
        }

        public override void Start()
        {
            this._rateLimitRemaining = 90;
            this._rateLimitReset = DateTime.Now.Ticks;
            
            base.Start();
        }

        public override async void Work()
        {
            base.Work();

            try
            {
                this._puppeteerClient = await this.initPuppeteer();
                this._spotifyClient = await this.initSpotifyWebAPI();

                foreach (string formatFilter in this._formatsFilter)
                {
                    this._anilistQuery.Variables["format"] = formatFilter;

                    for (int currentPage = 1; currentPage <= this._totalPages; currentPage++)
                    {
                        this._anilistQuery.Variables["page"] = currentPage;

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            Content = new StringContent(JsonConvert.SerializeObject(this._anilistQuery), Encoding.UTF8, "application/json")
                        };

                        try
                        {
                            using (var response = await this._anilistClient.SendAsync(request))
                            {
                                try
                                {
                                    response.EnsureSuccessStatusCode();
                                }
                                catch (Exception ex)
                                {
                                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                                    {
                                        this._rateLimitReset = Convert.ToInt64(((string[])response.Headers.GetValues("X-RateLimit-Reset"))[0]);
                                    }

                                    throw new HttpRequestException("RateLimit superato", ex);
                                }

                                AnilistResponse anilistResponse = JsonConvert.DeserializeObject<AnilistResponse>(await response.Content.ReadAsStringAsync());

                                if (currentPage == 1)
                                {
                                    this._totalPages = anilistResponse.Data.Page.PageInfo.LastPage;
                                }

                                foreach (AnilistResponse.ResponseMedia m in anilistResponse.Data.Page.Media)
                                {
                                    Anime anime = new Anime(m);

                                    if (this._animeCollection.Exists(ref anime))
                                    {
                                        if (anime.Status == AnimeStatusEnum.RELEASING)
                                        {
                                            anime = await this.fetchAniPlaylist(anime);
                                        }

                                        this._animeCollection.Edit(ref anime);
                                    }
                                    else
                                    {
                                        anime = await this.fetchAniPlaylist(anime);
                                        this._animeCollection.Add(ref anime);
                                    }
                                }

                                this._rateLimitRemaining = Convert.ToInt32(((string[])response.Headers.GetValues("X-RateLimit-Remaining"))[0]);

                                this.Log($"Format {formatFilter} done {GetProgress(currentPage, this._totalPages)}%");
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            currentPage--;

                            DateTime timeOfReset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            timeOfReset = timeOfReset.AddSeconds(this._rateLimitReset).ToLocalTime();

                            TimeSpan timeToWait = timeOfReset - DateTime.Now;

                            this.Log($"Waiting {timeToWait.TotalMilliseconds.ToString("F0")} ms!");

                            Thread.Sleep((int)timeToWait.TotalMilliseconds + 1000);
                        }
                    }
                }

                this.Wait();
            }
            catch(Exception ex)
            {
                this.Stop();
            }
        }

        private async Task<Browser> initPuppeteer()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            return await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true
            });
        }

        private async Task<SpotifyClient> initSpotifyWebAPI()
        {
            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
            string accessToken = (await new OAuthClient(config).RequestToken(new ClientCredentialsRequest("b074c52808fb4394a28785f381872ea2", "ac2f818c3d0c463a9b3056dc5ed489fc"))).AccessToken;
            return new SpotifyClient(config.WithToken(accessToken));
        }

        private async Task<Anime> fetchAniPlaylist(Anime anime)
        {
            try
            {
                using (Page page = await this._puppeteerClient.NewPageAsync())
                {
                    string url = $"https://aniplaylist.com/{Uri.EscapeUriString(anime.Titles[LocalizationEnum.English])}?types=Opening~Ending";
                    await page.GoToAsync(url);

                    await page.WaitForSelectorAsync(".song-card", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 3000
                    });

                    ElementHandle[] songs = new ElementHandle[0];
                    int lastSongsCount = 0;

                    do
                    {
                        lastSongsCount = songs.Length;
                        string windowHeight = "Math.max(" +
                            "document.body.scrollHeight, " +
                            "document.body.offsetHeight, " +
                            "document.documentElement.clientHeight, " +
                            "document.documentElement.scrollHeight, " +
                            "document.documentElement.offsetHeight" +
                        ")";
                        await page.EvaluateExpressionAsync($"window.scrollBy(0, {windowHeight})");
                        
                        Thread.Sleep(200);

                        songs = await page.QuerySelectorAllAsync(".song-card");
                    } while (lastSongsCount != songs.Length);

                    int maxOpeningVersion = 0, maxEndingVersion = 0;
                    string openingId = string.Empty, endingId = string.Empty;

                    foreach (ElementHandle song in songs)
                    {
                        if (anime.Opening != null && anime.Ending != null)
                        {
                            continue;
                        }

                        ElementHandle title = await song.QuerySelectorAsync(".card-image a .image .card-anime-title");
                        string songAnimeTitle = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                        if (anime.Titles[LocalizationEnum.English].ToLower() == songAnimeTitle.ToLower())
                        {
                            ElementHandle type = await song.QuerySelectorAsync(".card-content span.tag.is-primary");
                            string songType = (await type.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                            ElementHandle anchor = await song.QuerySelectorAsync(".card-image a");
                            string[] spotifyUrl = (await anchor.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Split('/');
                            string trackId = spotifyUrl[spotifyUrl.Length - 1];

                            if (songType.Contains("Opening") || songType.Contains("Ending"))
                            {
                                string[] parts = songType.Split(' ');

                                if (parts.Length == 1)
                                {
                                    parts = new string[2] { parts[0], "0" };
                                }

                                if (int.TryParse(parts[1], out int version))
                                {
                                    if (songType.Contains("Opening") && version >= maxOpeningVersion)
                                    {
                                        maxOpeningVersion = version;
                                        openingId = trackId;
                                    }
                                    else if (songType.Contains("Ending") && version >= maxEndingVersion)
                                    {
                                        maxEndingVersion = version;
                                        endingId = trackId;
                                    }
                                }
                            }
                        }
                    }

                    List<string> ids = new List<string>();
                    if (!string.IsNullOrEmpty(openingId))
                    {
                        ids.Add(openingId);
                    }

                    if (!string.IsNullOrEmpty(endingId))
                    {
                        ids.Add(endingId);
                    }

                    if (ids.Count > 0)
                    {
                        TracksResponse spotifyResponse = await this._spotifyClient.Tracks.GetSeveral(new TracksRequest(ids));

                        foreach (FullTrack track in spotifyResponse.Tracks)
                        {
                            string[] dateParts = track.Album.ReleaseDate.Split('-');

                            AnimeSong animeSong = new AnimeSong()
                            {
                                Title = track.Name,
                                Artist = track.Artists[0].Name,
                                Album = track.Album.Name,
                                Duration = track.DurationMs,
                                SpotifyUrl = track.Href,
                                PreviewUrl = track.PreviewUrl,
                                Year = Convert.ToInt32(dateParts[0])
                            };

                            if (dateParts.Length > 1)
                            {
                                animeSong.SetSeason(Convert.ToInt32(dateParts[1]));
                            }

                            if (track.Id == openingId)
                            {
                                anime.Opening = animeSong;
                            }
                            else if (track.Id == endingId)
                            {
                                anime.Ending = animeSong;
                            }
                        }
                    }
                }
            }
            catch { }

            return anime;
        }
    }
}
