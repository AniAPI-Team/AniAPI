using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Models
{
    public abstract class IWebsiteScraper
    {
        private AnimeCollection _animeCollection = new AnimeCollection();
        private EpisodeCollection _episodeCollection = new EpisodeCollection();

        protected abstract long WebsiteID { get; }
        protected Website Website { get; private set; }
        protected abstract Type WebsiteType { get; }
        protected WebsiteScraperService Service { get; set; }

        protected Thread Thread { get; private set; }
        public bool Working { get; private set; }

        public IWebsiteScraper(WebsiteScraperService service)
        {
            this.Service = service;
            this.Website = new WebsiteCollection().Get(this.WebsiteID);
        }

        public void Start()
        {
            this.Thread = new Thread(new ThreadStart(run));
            this.Thread.IsBackground = true;

            this.Thread.Start();
            this.Working = true;
        }

        protected abstract Task<AnimeMatching> GetMatching(Page webPage, string animeTitle);
        protected abstract Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number);

        private async void run()
        {
            string browserKey = ProxyHelper.Instance.GenerateBrowserKey(this.WebsiteType);

            try
            {
                long lastID = this._animeCollection.Last().Id;
                Anime anime = null;

                for (int animeID = 1; animeID < lastID; animeID++)
                {
                    try
                    {
                        anime = this._animeCollection.Get(animeID);

                        if (!this.animeNeedWork(anime))
                        {
                            throw new Exception();
                        }

                        AnimeMatching matching = null;
                        using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browserKey, this.Website.CanBlockRequests))
                        {
                            string animeTitle = anime.Titles.ContainsKey(this.Website.Localization) ?
                                anime.Titles[this.Website.Localization] :
                                anime.Titles[LocalizationEnum.English];

                            matching = await this.GetMatching(webPage, animeTitle);
                        }

                        if (matching == null)
                        {
                            throw new Exception();
                        }

                        try
                        {
                            for (int i = 1; i <= anime.EpisodesCount; i++)
                            {
                                using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browserKey, this.Website.CanBlockRequests))
                                {
                                    EpisodeMatching episode = await this.GetEpisode(webPage, matching, i);

                                    if (episode != null)
                                    {
                                        matching.Episodes.Add(episode);
                                    }
                                }
                            }
                        }
                        catch { }

                        if (this.Website.Official)
                        {
                            anime.Titles[this.Website.Localization] = matching.Title;
                            anime.Descriptions[this.Website.Localization] = matching.Description;

                            this._animeCollection.Edit(ref anime);
                        }

                        if (matching.Episodes.Count > 0)
                        {
                            foreach (EpisodeMatching episode in matching.Episodes)
                            {
                                Episode ep = new Episode()
                                {
                                    AnimeID = anime.Id,
                                    Source = this.Website.Name,
                                    Number = episode.Number,
                                    Title = episode.Title,
                                    Video = episode.Source
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
                        string animeTitle = anime.Titles[LocalizationEnum.English];
                        this.Service.Log($"Website {this.Website.Name} done {this.Service.GetProgressD(animeID, lastID)}% ({animeTitle})");
                    }
                }
            }
            catch(Exception ex)
            {
                this.Service.Log($"Error: {ex.Message}");
            }
            finally
            {
                ProxyHelper.Instance.CloseProxy(browserKey);
                this.Working = false;
            }
        }

        private bool animeNeedWork(Anime anime)
        {
            if(anime.Status == AnimeStatusEnum.RELEASING)
            {
                return true;
            }

            long episodesCount = this._episodeCollection.GetList<EpisodeFilter>(new EpisodeFilter()
            {
                anime_id = anime.Id,
                source = this.Website.Name
            }).Count;

            if (episodesCount == 0 || (anime.Status == AnimeStatusEnum.FINISHED && anime.EpisodesCount != episodesCount))
            {
                return true;
            }

            return false;
        }
    }
}
