using Commons;
using Commons.Collections;
using Commons.Enums;
using PuppeteerSharp;
using SpotifyAPI.Web;
using SyncService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class SongScraperService : IService
    {
        #region Members

        private AnimeCollection _animeCollection = new AnimeCollection();
        private SpotifyClient _spotifyClient;
        private long _lastId = -1;

        protected override int TimeToWait => 60 * 1000 * 60 * 24; // 24 Hours

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("SongScraper");
        }

        public override async void Start()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
            string accessToken = (await new OAuthClient(config).
                RequestToken(new ClientCredentialsRequest(
                    "b074c52808fb4394a28785f381872ea2", 
                    "ac2f818c3d0c463a9b3056dc5ed489fc"
                ))).AccessToken;
            this._spotifyClient = new SpotifyClient(config.WithToken(accessToken));

            this._lastId = this._animeCollection.Last().Id;

            base.Start();
        }

        public override async void Work()
        {
            base.Work();

            string browserKey = ProxyHelper.Instance.GenerateBrowserKey(typeof(SongScraperService));

            try
            {
                for (int id = 1; id < this._lastId; id++)
                {
                    try
                    {
                        Anime anime = this._animeCollection.Get(id);

                        if (anime.Opening == null || anime.Ending == null || anime.Status == AnimeStatusEnum.RELEASING)
                        {
                            using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browserKey, true))
                            {
                                string url = $"https://aniplaylist.com/{Uri.EscapeUriString(anime.Titles[LocalizationEnum.English])}?types=Opening~Ending";
                                await webPage.GoToAsync(url);

                                await webPage.WaitForSelectorAsync(".song-card", new WaitForSelectorOptions()
                                {
                                    Visible = true,
                                    Timeout = 2000
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
                                    await webPage.EvaluateExpressionAsync($"window.scrollBy(0, {windowHeight})");

                                    Thread.Sleep(200);

                                    songs = await webPage.QuerySelectorAllAsync(".song-card");
                                } while (lastSongsCount != songs.Length);

                                int maxOpeningVersion = 0, maxEndingVersion = 0;
                                string openingId = string.Empty, endingId = string.Empty;

                                foreach (ElementHandle song in songs)
                                {
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
                                            OpenSpotifyUrl = $"https://open.spotify.com/track/{track.Id}",
                                            LocalSpotifyUrl = track.Uri,
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

                                this._animeCollection.Edit(ref anime);
                            }
                        }

                        this.Log($"Done {GetProgressD(id, this._lastId)}% ({anime.Titles[LocalizationEnum.English]})");
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                this.Stop(ex);
            }
        }
    }
}
