using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PuppeteerSharp;
using SpotifyAPI.Web;
using SyncService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class SongScraperService : IService
    {
        #region Members

        private AnimeCollection _animeCollection = new AnimeCollection();
        private AnimeSongCollection _animeSongCollection = new AnimeSongCollection();
        
        private SpotifyClient _spotifyClient;
        private Anime _anime;
        private HttpClient _scraperClient;

        protected override int TimeToWait => 60 * 1000 * 60 * 24; // 24 Hours

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("SongScraper");
        }

        public override async Task Start(CancellationToken cancellationToken)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfiguration config = new ConfigurationBuilder().
                AddJsonFile("appsettings.json", false, false).
                AddJsonFile($"appsettings.{env}.json", false, false).
                AddEnvironmentVariables().
                Build();

            SpotifyClientConfig spotifyConfig = SpotifyClientConfig.CreateDefault();
            string accessToken = (await new SpotifyAPI.Web.OAuthClient(spotifyConfig).
                RequestToken(new ClientCredentialsRequest(
                    "b074c52808fb4394a28785f381872ea2",
                    "ac2f818c3d0c463a9b3056dc5ed489fc"
                ))).AccessToken;
            this._spotifyClient = new SpotifyClient(spotifyConfig.WithToken(accessToken));

            this._scraperClient = new HttpClient
            {
                BaseAddress = new Uri(config.GetValue<string>("ScraperEngine:Url"))
            };

            await base.Start(cancellationToken);
        }

        public override async Task Work()
        {
            await base.Work();

            try
            {
                long lastId = this._animeCollection.Last().Id;

                for (int id = 1; id < lastId; id++)
                {
                    try
                    {
                        _anime = this._animeCollection.Get(id);

                        if (!this.animeNeedWork())
                        {
                            throw new Exception();
                        }

                        List<AnimeSong> animeSongs = new List<AnimeSong>();

                        string url = $"/song/{Uri.EscapeUriString(_anime.Titles[LocalizationEnum.English])}";
                        using (var response = await _scraperClient.GetAsync(url))
                        {
                            try
                            {
                                response.EnsureSuccessStatusCode();
                            }
                            catch
                            {
                                throw;
                            }

                            List<ScraperSongResponse> songs = JsonConvert.DeserializeObject<List<ScraperSongResponse>>(await response.Content.ReadAsStringAsync());

                            foreach(var song in songs)
                            {
                                AnimeSongTypeEnum animeSongType = AnimeSongTypeEnum.NONE;

                                if (song.Type == "Opening")
                                {
                                    animeSongType = AnimeSongTypeEnum.OPENING;
                                }
                                else if (song.Type == "Ending")
                                {
                                    animeSongType = AnimeSongTypeEnum.ENDING;
                                }

                                if (animeSongType != AnimeSongTypeEnum.NONE && !animeSongs.Select(x => x.SpotifyID).ToList().Contains(song.Id))
                                {
                                    animeSongs.Add(new AnimeSong()
                                    {
                                        AnimeID = _anime.Id,
                                        SpotifyID = song.Id,
                                        SongType = animeSongType
                                    });
                                }
                            }
                        }

                        List<string> ids = animeSongs.Select(x => x.SpotifyID).ToList();

                        if (ids.Count > 0)
                        {
                            bool done = false;
                            TracksResponse spotifyResponse = null;

                            while (done == false)
                            {
                                try
                                {
                                    spotifyResponse = await this._spotifyClient.Tracks.GetSeveral(new TracksRequest(ids));
                                    done = true;
                                }
                                catch
                                {
                                    SpotifyClientConfig config = SpotifyClientConfig.CreateDefault();
                                    string accessToken = (await new SpotifyAPI.Web.OAuthClient(config).
                                        RequestToken(new ClientCredentialsRequest(
                                            "b074c52808fb4394a28785f381872ea2",
                                            "ac2f818c3d0c463a9b3056dc5ed489fc"
                                        ))).AccessToken;
                                    this._spotifyClient = new SpotifyClient(config.WithToken(accessToken));
                                }
                            }

                            foreach (FullTrack track in spotifyResponse.Tracks)
                            {
                                string[] dateParts = track.Album.ReleaseDate.Split('-');

                                AnimeSong animeSong = animeSongs.FirstOrDefault(x => x.SpotifyID == track.Id);

                                animeSong.Title = track.Name;
                                animeSong.Artist = track.Artists[0].Name;
                                animeSong.Album = track.Album.Name;
                                animeSong.Duration = track.DurationMs;
                                animeSong.OpenSpotifyUrl = $"https://open.spotify.com/track/{track.Id}";
                                animeSong.LocalSpotifyUrl = track.Uri;
                                animeSong.PreviewUrl = track.PreviewUrl;
                                animeSong.Year = Convert.ToInt32(dateParts[0]);

                                if (dateParts.Length > 1)
                                {
                                    animeSong.SetSeason(Convert.ToInt32(dateParts[1]));
                                }
                                else
                                {
                                    animeSong.Season = AnimeSeasonEnum.UNKNOWN;
                                }

                                if (this._animeSongCollection.Exists(ref animeSong))
                                {
                                    this._animeSongCollection.Edit(ref animeSong);
                                }
                                else
                                {
                                    this._animeSongCollection.Add(ref animeSong);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                    finally
                    {
                        this.Log($"Done {GetProgressD(id, lastId)}% ({_anime.Titles[LocalizationEnum.English]})", true);
                    }

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Process cancellation requested!");
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private bool animeNeedWork()
        {
            if(_anime.Format != AnimeFormatEnum.TV)
            {
                return false;
            }

            if (_anime.Status == AnimeStatusEnum.RELEASING)
            {
                return true;
            }

            long songsCount = this._animeSongCollection.GetList<AnimeSongFilter>(new AnimeSongFilter()
            {
                anime_id = _anime.Id
            }).Count;

            if(songsCount == 0)
            {
                return true;
            }

            return false;
        }
    }
}
