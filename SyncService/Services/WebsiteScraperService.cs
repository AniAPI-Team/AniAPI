using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using FuzzySharp;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoService;
using Newtonsoft.Json;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Models;
using SyncService.Models.Websites;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SyncService.Services
{
    public class WebsiteScraperService : IService
    {
        #region Members

        private AppSettings _appSettings;
        private AnimeCollection _animeCollection = new AnimeCollection();
        private EpisodeCollection _episodeCollection = new EpisodeCollection();
        private AnimeSuggestionCollection _animeSuggestionCollection = new AnimeSuggestionCollection();

        private Dictionary<string, List<IWebsite>> _websites = new Dictionary<string, List<IWebsite>>();
        private Anime _anime;
        private HttpClient _scraperClient;

        private int _proxyCounter = 0;

        protected override int TimeToWait => 1000 * 60 * 10; // 10 Minutes

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("WebsiteScraper");
        }

        public override async Task Start(CancellationToken cancellationToken)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfiguration config = new ConfigurationBuilder().
                AddJsonFile("appsettings.json", false, false).
                AddJsonFile($"appsettings.{env}.json", false, false).
                AddEnvironmentVariables().
                Build();

            var websites = new WebsiteCollection().Collection
                .Find(Builders<Website>.Filter.Eq("active", true))
                .ToList();

            foreach(var website in websites)
            {
                if (!_websites.ContainsKey(website.Localization))
                {
                    _websites[website.Localization] = new List<IWebsite>();
                }

                IWebsite iWeb = null;

                switch (website.Name)
                {
                    case "dreamsub":
                        iWeb = new DreamsubWebsite(website);
                        break;
                    case "animeworld":
                        iWeb = new AnimeworldWebsite(website);
                        break;
                    case "gogoanime":
                        iWeb = new GogoanimeWebsite(website);
                        break;
                    case "desuonline":
                        iWeb = new DesuonlineWebsite(website);
                        break;

                    default:
                        throw new Exception($"Website {website.Name} not handled!");
                }
                _websites[website.Localization].Add(iWeb);

#if DEBUG
                await BenchmarkHelper.Start(website.Name);
#endif
            }

            this._appSettings = new AppSettingsCollection().Get(0);
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

                for(long animeID = 1; animeID < lastId; animeID++)
                {
                    try
                    {
                        _anime = this._animeCollection.Get(animeID);

#if DEBUG
                        this.Log($"Doing {_anime}", true);
#endif

                        Task[] tasks = new Task[_websites.Count];
                        int i = 0;
                        foreach(string localization in _websites.Keys)
                        {
                            Task task = Task.Factory.StartNew(() =>
                            {
                                start(_websites[localization], _anime);
                            });
                            
                            tasks[i] = task;
                            i++;
                        }

                        await Task.WhenAll(tasks);
                    }
                    catch(Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                    finally
                    {
                        this.Log($"Done {GetProgressD(animeID, lastId)}% [{_anime}({_anime.Id})]", true);
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

        private void start(List<IWebsite> websites, Anime anime)
        {
            foreach(IWebsite website in websites)
            {
                run(website, anime).Wait();
            }
        }

        private async Task run(IWebsite website, Anime anime)
        {
#if DEBUG
            BenchmarkHelper.BenchmarkData benchmarkMatching = new BenchmarkHelper.BenchmarkData
            {
                Anime = anime.Titles[LocalizationEnum.English],
                Operation = "matching"
            };
#endif

            try
            {
                AnimeMatching matching = null;

                string animeTitle = _anime.Titles.ContainsKey(website.Localization) ?
                    _anime.Titles[website.Localization] : _anime.Titles[LocalizationEnum.Romaji];

#if DEBUG
                var watcher = Stopwatch.StartNew();
#endif

                string url = $"/{website.Name}/matchings?title={HttpUtility.UrlEncode(animeTitle)}";
                using (var response = await _scraperClient.GetAsync(url))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
#if DEBUG
                        benchmarkMatching.Error = true;
#endif
                        throw new ScrapingException($"[{website.Name}] error while matching");
                    }

#if DEBUG
                    watcher.Stop();
                    benchmarkMatching.ResponseTime = watcher.Elapsed;
#endif
                    ScraperMatchingResponse matchings = JsonConvert.DeserializeObject<ScraperMatchingResponse>(await response.Content.ReadAsStringAsync());

#if DEBUG
                    benchmarkMatching.RequestSize = matchings.Size;
#endif

                    matchings.Data.OrderBy(x => x.Title).ToList().ForEach(m =>
                    {
                        AnimeMatching match = new AnimeMatching
                        {
                            Title = m.Title,
                            Path = m.Path
                        };

                        if(website.AnalyzeMatching(anime, match, animeTitle))
                        {
                            if (match.IsDub && matching != null)
                            {
                                matching.Linked = match;
                            }
                            else if(match.IsDub && matching == null)
                            {
                                matching = match;
                            }
                            else
                            {
                                matching = match;
                            }
                        }
                    });
                }

                if (matching == null)
                {
                    throw new ScrapingException($"[{website.Name}] not found [{_anime}({_anime.Id})]");
                }

#if DEBUG
                this.Log($"[{website.Name}] found {matching.Title} [{_anime}({_anime.Id})]");
#endif

                if (website.Official)
                {
                    if (!_anime.Titles.ContainsKey(website.Localization))
                    {
                        _anime.Titles[website.Localization] = matching.Title;
                    }
                    else if (string.IsNullOrEmpty(_anime.Titles[website.Localization]))
                    {
                        _anime.Titles[website.Localization] = matching.Title;
                    }

                    if (!_anime.Descriptions.ContainsKey(website.Localization))
                    {
                        _anime.Descriptions[website.Localization] = matching.Description;
                    }
                    else if (string.IsNullOrEmpty(_anime.Descriptions[website.Localization]))
                    {
                        _anime.Descriptions[website.Localization] = matching.Description;
                    }
                }

                _anime.HasEpisodes = true;
                _animeCollection.Edit(ref _anime);

                while (matching != null)
                {
                    List<Episode> dbEpisodes = this._episodeCollection.Collection
                        .Find(x => x.AnimeID == _anime.Id && 
                            x.Locale == website.Localization &&
                            x.IsDub == matching.IsDub)
                        .ToList();

                    for(int i = 1; i <= _anime.EpisodesCount; i++)
                    {
                        bool isBroken = false;

                        foreach(Episode dbEpisode in dbEpisodes.Where(x => x.Number == i))
                        {
#if DEBUG
                            watcher.Restart();
#endif
                            _proxyCounter = (_proxyCounter + 1) > _appSettings.ProxyCount ? 0 : _proxyCounter + 1;

                            var proxy = new WebProxy
                            {
                                Address = new Uri($"http://{_appSettings.ProxyHost}:{_appSettings.ProxyPort}"),
                                BypassProxyOnLocal = false,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential($"{_appSettings.ProxyUsername}{_proxyCounter}", _appSettings.ProxyPassword)
                            };

                            HttpClientHandler handler = new HttpClientHandler()
                            {
                                ClientCertificateOptions = ClientCertificateOption.Manual,
                                Proxy = proxy,
                                ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) =>
                                {
                                    return true;
                                }
                            };

                            HttpClient checkClient = new HttpClient(handler)
                            {
                                MaxResponseContentBufferSize = 1024
                            };

                            try
                            {
                                var request = new HttpRequestMessage(HttpMethod.Head, dbEpisode.Video);

                                request.Headers.IfModifiedSince = null;

                                if (dbEpisode.VideoHeaders != null)
                                {
                                    foreach(string k in dbEpisode.VideoHeaders.Keys)
                                    {
                                        request.Headers.Add(k, dbEpisode.VideoHeaders[k]);
                                    }
                                }

                                using(var response = await checkClient.SendAsync(request))
                                {
                                    response.EnsureSuccessStatusCode();

#if DEBUG
                                    this.Log($"[{website.Name}] checked episode {i} [{_anime}({anime.Id})]");
#endif
                                }
                            }
                            catch(Exception ex)
                            {
                                if(ex.InnerException != null)
                                {
                                    isBroken = true;
#if DEBUG
                                    this.Log($"[{website.Name}] found broken episode {i} [{_anime}({_anime.Id})]");
#endif
                                }
                            }

#if DEBUG
                            watcher.Stop();
                            await BenchmarkHelper.Track(website.Name, new BenchmarkHelper.BenchmarkData
                            {
                                Anime = matching.Title + (matching.IsDub ? "(dub)" : string.Empty),
                                Operation = $"check_episode_{i}",
                                RequestSize = 0,
                                ResponseTime = watcher.Elapsed,
                                Error = false
                            });
#endif

                            checkClient.Dispose();
                        }

                        if(dbEpisodes.Where(x => x.Number == i).Count() == 0 || isBroken)
                        {
#if DEBUG
                            BenchmarkHelper.BenchmarkData benchmarkEpisode = new BenchmarkHelper.BenchmarkData
                            {
                                Anime = matching.Title,
                                Operation = $"get_episode_{i}"
                            };
                            watcher.Restart();
#endif

                            url = $"/{website.Name}/episode?path={HttpUtility.UrlEncode(matching.Path)}&number={i}";
                            using (var response = await _scraperClient.GetAsync(url))
                            {
                                try
                                {
                                    response.EnsureSuccessStatusCode();
                                }
                                catch
                                {
#if DEBUG
                                    watcher.Stop();
                                    
                                    benchmarkEpisode.Error = true;
                                    benchmarkEpisode.ResponseTime = watcher.Elapsed;
                                    await BenchmarkHelper.Track(website.Name, benchmarkEpisode);

                                    this.Log($"[{website.Name}] error while getting episode {i} [{_anime}({_anime.Id})]");
#endif
                                    continue;
                                }

#if DEBUG
                                watcher.Stop();
                                benchmarkEpisode.ResponseTime = watcher.Elapsed;
#endif

                                ScraperEpisodeResponse episodeQualities = JsonConvert.DeserializeObject<ScraperEpisodeResponse>(await response.Content.ReadAsStringAsync());

#if DEBUG
                                benchmarkEpisode.RequestSize = episodeQualities.Size;
                                await BenchmarkHelper.Track(website.Name, benchmarkEpisode);
#endif

                                episodeQualities.Data.ForEach(epQuality =>
                                {
                                    matching.EpisodePath = epQuality.Path;

                                    Episode ep = new Episode
                                    {
                                        AnimeID = _anime.Id,
                                        Number = i,
                                        Title = epQuality.Title,
                                        Video = epQuality.Video,
                                        VideoHeaders = website.GetVideoProxyHeaders(matching, null),
                                        //Video = website.BuildAPIProxyURL(_appSettings, matching, epQuality.Video, null),
                                        Quality = epQuality.Quality,
                                        Format = epQuality.Format,
                                        IsDub = matching.IsDub,
                                        Locale = website.Localization,
                                        Source = website.Name
                                    };

                                    if (this._episodeCollection.Exists(ref ep))
                                    {
                                        this._episodeCollection.Edit(ref ep);
                                    }
                                    else
                                    {
                                        this._episodeCollection.TryInsertWithRetry(ref ep);
                                    }
                                });

#if DEBUG
                                this.Log($"[{website.Name}] done episode {i} [{_anime}({_anime.Id})]");
#endif
                            }
                        }
                        else
                        {
                            dbEpisodes.Where(x => x.Number == i).ToList().ForEach(ep =>
                            {
                                ep.Source = website.Name;

                                if(this._episodeCollection.Exists(ref ep))
                                {
                                    this._episodeCollection.Edit(ref ep);
                                }
                            });
                        }
                    }

                    matching = matching.Linked ?? null;
                }
            }
            catch(ScrapingException ex)
            {
#if DEBUG
                this.Log($"Scraper: {ex.Message} [{_anime}({_anime.Id})]");
#endif
            }
            catch(Exception ex)
            {
                this.Log($"Error: {ex.Message}");
                this.Log(ex.StackTrace);
            }

#if DEBUG
            await BenchmarkHelper.Track(website.Name, benchmarkMatching);
#endif
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        public class ScrapingException : Exception
        {
            public ScrapingException() { }
            public ScrapingException(string message) : base(message) { }
        }
    }
}
