﻿using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using FuzzySharp;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SyncService.Models
{
    public abstract class IWebsiteScraper
    {
        private AppSettings _appSettings;
        private AnimeCollection _animeCollection = new AnimeCollection();
        private EpisodeCollection _episodeCollection = new EpisodeCollection();
        private AnimeSuggestionCollection _animeSuggestionCollection = new AnimeSuggestionCollection();
        private Anime _anime;

        protected abstract long WebsiteID { get; }
        protected Website Website { get; private set; }
        protected abstract Type WebsiteType { get; }
        protected virtual bool WebsiteForceReload { get; } = false;
        protected WebsiteScraperService Service { get; set; }
        protected Thread Thread { get; private set; }
        public bool Working { get; private set; } = false;

        protected List<EpisodeMatching> EpisodeMatchings = new List<EpisodeMatching>();

        public IWebsiteScraper(WebsiteScraperService service)
        {
            this.Service = service;
            this.Website = new WebsiteCollection().Get(this.WebsiteID);

            this._appSettings = new AppSettingsCollection().Get(0);
        }

        public async Task Start()
        {
            if (this.Website.Active)
            {
                this.Working = true;
                await this.run();
            }
        }

        protected bool AnalyzeMatching(AnimeMatching matching, string sourceTitle)
        {
            matching.Score = Fuzz.TokenSortRatio(matching.Title, sourceTitle);

            if(matching.Score == 100)
            {
                return true;
            }
            else if(matching.Score >= 60)
            {
                var query = this._animeSuggestionCollection.GetList(new AnimeSuggestionFilter()
                {
                    anime_id = _anime.Id,
                    title = matching.Title,
                    source = this.Website.Name
                });

                if(query.Count > 0)
                {
                    if(query.Documents[0].Status == AnimeSuggestionStatusEnum.OK)
                    {
                        return true;
                    }
                    else if(query.Documents[0].Status == AnimeSuggestionStatusEnum.KO)
                    {
                        return false;
                    }
                }
                else
                {
                    AnimeSuggestion suggestion = new AnimeSuggestion()
                    {
                        AnimeID = _anime.Id,
                        Title = matching.Title,
                        Source = this.Website.Name,
                        Score = matching.Score,
                        Path = $"{this.Website.SiteUrl}{matching.Path}",
                        Status = AnimeSuggestionStatusEnum.NONE
                    };

                    this._animeSuggestionCollection.Add(ref suggestion);
                }
            }

            return false;
        }

        protected string BuildAPIProxyURL(string url, Dictionary<string, string> values = null)
        {
            string apiUrl = $"{_appSettings.APIEndpoint}/proxy/{HttpUtility.UrlEncode(url)}/{this.Website.Name}/";

            if(values != null)
            {
                apiUrl += "?";
            }

            for(int i = 0; i < values?.Keys.Count; i++)
            {
                string key = values.Keys.ElementAt(i);

                apiUrl += $"{key}={HttpUtility.UrlEncode(values[key])}";

                if((i + 1) < values.Keys.Count)
                {
                    apiUrl += "&";
                }
            }

            return apiUrl;
        }

        protected abstract Task<AnimeMatching> GetMatching(Page webPage, string animeTitle);
        protected abstract Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number);


        private async Task run()
        {
            try
            {
                long lastID = this._animeCollection.Last().Id;

                for (long animeID = 1; animeID < lastID; animeID++)
                {
                    try
                    {
                        _anime = this._animeCollection.Get(animeID);

                        if(_anime == null)
                        {
                            continue;
                        }

                        this.Service.Log($"Website {this.Website.Name} doing {_anime.Titles[LocalizationEnum.English]}", true);

                        if (!this.animeNeedWork(this.WebsiteForceReload))
                        {
                            throw new ScrapingException($"Website {this.Website.Name} skipped {_anime}");
                        }

                        AnimeMatching matching = null;
                        Browser browser = await ProxyHelper.Instance.GetBrowser();
                        try
                        {
                            using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browser, this.Website.CanBlockRequests))
                            {
                                string animeTitle = _anime.Titles.ContainsKey(this.Website.Localization) ?
                                    _anime.Titles[this.Website.Localization] :
                                    _anime.Titles[LocalizationEnum.English];

                                matching = await this.GetMatching(webPage, animeTitle);
                            }
                        }
                        catch { }
                        finally
                        {
                            await browser.CloseAsync();
                        }

                        if (matching == null)
                        {
                            throw new ScrapingException($"Website {this.Website.Name} not found {_anime}");
                        }
                        
                        try
                        {
                            AnimeMatching linked = matching.Linked;
                            
                            await getEpisodes(browser, matching);

                            while (linked != null)
                            {
                                EpisodeMatchings.Clear();

                                await getEpisodes(browser, linked);
                                linked = linked.Linked;
                            }
                        }
                        catch 
                        {
                            throw new ScrapingException($"Website {this.Website.Name} no episode found ({_anime})");
                        }

                        if (this.Website.Official)
                        {
                            _anime.Titles[this.Website.Localization] = matching.Title;
                            _anime.Descriptions[this.Website.Localization] = matching.Description;

                            this._animeCollection.Edit(ref _anime);
                        }
                    }
                    catch(ScrapingException ex) 
                    {
#if DEBUG
                        this.Service.Log(ex.Message);
#endif
                        this.Service.Error(ex.Message);
                    }
                    catch (Exception ex) 
                    {
                        this.Service.Log($"Error: {ex.Message}");
                        this.Service.Log(ex.StackTrace);
                    }
                    finally
                    {
                        this.Service.Log($"Website {this.Website.Name} done {this.Service.GetProgressD(animeID, lastID)}% ({_anime.Titles[LocalizationEnum.English]})", true);
                    }

                    if (this.Service._cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Process cancellation requested!");
                    }
                }
            }
            catch(TaskCanceledException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                this.Service.Log($"Error: {ex.Message}");
                this.Service.Log(ex.StackTrace);
            }
            finally
            {
                this.Working = false;
            }
        }

        private async Task getEpisodes(Browser browser, AnimeMatching matching)
        {
            for (int i = 1; i <= _anime.EpisodesCount; i++)
            {
                browser = await ProxyHelper.Instance.GetBrowser();
                try
                {
                    using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browser, this.Website.CanBlockRequests))
                    {
                        EpisodeMatching episode = await this.GetEpisode(webPage, matching, i);

                        if (episode != null)
                        {
                            Episode ep = new Episode()
                            {
                                AnimeID = _anime.Id,
                                Source = $"{this.Website.Name}{matching.SourceVariant}",
                                Number = episode.Number,
                                Title = episode.Title,
                                Video = episode.Source,
                                Locale = this.Website.Localization
                            };

                            if (this._episodeCollection.Exists(ref ep))
                            {
                                this._episodeCollection.Edit(ref ep);
                            }
                            else
                            {
                                this._episodeCollection.Add(ref ep);
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    await browser.CloseAsync();
                }
            }
        }

        private bool animeNeedWork(bool forceReload = false)
        {
            if (forceReload)
            {
                return true;
            }

            long episodesCount = this._episodeCollection.GetList<EpisodeFilter>(new EpisodeFilter()
            {
                anime_id = _anime.Id,
                source = this.Website.Name,
                locale = this.Website.Localization
            }).Count;

            if ((_anime.Status == AnimeStatusEnum.RELEASING || _anime.Status == AnimeStatusEnum.FINISHED) && _anime.EpisodesCount > episodesCount)
            {
                return true;
            }

            if(episodesCount == 0)
            {
                return true;
            }

            return false;
        }

        [Serializable]
        public class ScrapingException : Exception
        {
            public ScrapingException() { }
            public ScrapingException(string message) : base(message) { }
        }
    }
}
