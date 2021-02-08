using AngleSharp;
using AngleSharp.Dom;
using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using FuzzySharp;
using MongoService;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Models;
using SyncService.Models.WebsiteScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class WebsiteScraperService : IService
    {
        #region Members

        private List<IWebsiteScraper> _workers;

        protected override int TimeToWait => 1000 * 60 * 60 * 6; // 6 Hours

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("WebsiteScraper");
        }

        public override async void Work()
        {
            base.Work();

            try
            {
                this._workers = new List<IWebsiteScraper>()
                {
                    new DreamsubScraper(this)
                };

                foreach(IWebsiteScraper scraper in this._workers)
                {
                    scraper.Start();
                }

                int alives = this._workers.Where(x => x.Working == true).Count();

                while (alives > 0)
                {
                    alives = this._workers.Where(x => x.Working == true).Count();

                    Thread.Sleep(1000 * 60);
                }
            }
            catch(Exception ex)
            {
                this.Stop();
            }
            
            this.Wait();
        }

        /*
        private async void workerRun(object argument)
        {
            Website website = (Website)argument;
            string browserKey = ProxyHelper.Instance.GenerateBrowserKey(typeof(WebsiteScraperService));

            for (int id = 1; id < this._lastId; id++)
            {
                try
                {
                    Anime anime = this._animeCollection.Get(id);
                    AnimeMatching matching = new AnimeMatching();

                    if(!this.needWork(website, anime))
                    {
                        continue;
                    }

                    try
                    {
                        using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browserKey, website.CanBlockRequests))
                        {
                            //this.Log($"Searching \"{anime.Titles[LocalizationEnum.English]}\"");
                            matching = await this.getMatchings(webPage, anime, website);
                        }

                        if (matching != null)
                        {
                            //this.Log($"Matched \"{anime.Titles[LocalizationEnum.English]}\" with \"{matching.Title}\"");

                            if (website.ScrapeType == WebsiteScrapeType.EPISODES_DIFFERENT_PAGE)
                            {
                                foreach (EpisodeMatching episode in matching.Episodes)
                                {
                                    using (Page webPage = await ProxyHelper.Instance.GetBestProxy(browserKey, website.CanBlockRequests))
                                    {
                                        episode.Source = await this.getEpisodeSource(webPage, episode, website);

                                        if (string.IsNullOrEmpty(episode.Source))
                                        {
                                            throw new Exception();
                                        }

                                        //this.Log($"Anime {anime.Titles[LocalizationEnum.English]} done {GetProgressD(episode.Number, matching.Episodes.Count)}%");
                                        //this.Log($"Episode {episode.Number} source: {episode.Source}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            matching = new AnimeMatching();
                            throw new Exception();
                        }
                    }
                    catch (Exception ex)
                    {
                        //this.Log("No episodes found!");
                        matching.Completed = false;
                    }

                    if (website.Official)
                    {
                        anime.Titles[website.Localization] = matching.Title;
                        anime.Descriptions[website.Localization] = matching.Description;

                        this._animeCollection.Edit(ref anime);
                    }

                    if (matching.Completed)
                    {
                        foreach (EpisodeMatching episode in matching.Episodes)
                        {
                            Episode ep = new Episode()
                            {
                                AnimeID = anime.Id,
                                Source = website.Name,
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

                    this._workers.Keys.FirstOrDefault(x => x.Id == website.Id).LastLog = $"Website {website.Name} done {GetProgressD(id, this._lastId)}% ({anime.Titles[LocalizationEnum.English]})";
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            ProxyHelper.Instance.CloseProxy(browserKey);
            this._workers.Keys.FirstOrDefault(x => x.Id == website.Id).Working = false;
        }

        private bool needWork(Website website, Anime anime)
        {
            if(anime.Status == AnimeStatusEnum.RELEASING)
            {
                return true;
            }

            long episodesCount = this._episodeCollection.GetList<EpisodeFilter>(new EpisodeFilter()
            {
                anime_id = anime.Id,
                source = website.Name
            }).Count;

            if(episodesCount == 0)
            {
                return true;
            }
            
            return false;
        }

        private async Task<AnimeMatching> getMatchings(Page webPage, Anime anime, Website website)
        {
            AnimeMatching matching;
            string animeTitle = null;

            if (anime.Titles.ContainsKey(website.Localization))
            {
                animeTitle = anime.Titles[website.Localization];
            }
            else
            {
                animeTitle = anime.Titles[LocalizationEnum.English];
            }

            string url = $"{website.SiteUrl}{website.Search.Query}{Uri.EscapeUriString(animeTitle)}";
            await webPage.GoToAsync(url);

            await webPage.WaitForSelectorAsync(website.Search.WaitSelector, new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 3000
            });

            await webPage.ScreenshotAsync("matchings.png");

            foreach(ElementHandle element in await webPage.QuerySelectorAllAsync(website.Search.ElementsSelector))
            {
                matching = new AnimeMatching();

                ElementHandle title = await element.QuerySelectorAsync(website.Search.TitleSelector);
                matching.Title = (await title.EvaluateFunctionAsync<string>($"e => {website.Search.TitleFunction}")).Trim();

                if (!string.IsNullOrEmpty(website.Search.TitleReplace))
                {
                    matching.Title = matching.Title.Replace(website.Search.TitleReplace, string.Empty).Trim();
                }

                matching.Score = Fuzz.TokenSortRatio(matching.Title, animeTitle);

                if(matching.Score == 100)
                {
                    ElementHandle path = await element.QuerySelectorAsync(website.Search.PathSelector);
                    matching.Path = (await path.EvaluateFunctionAsync<string>($"e => {website.Search.PathFunction}")).Trim();

                    try
                    {
                        matching.Episodes = await this.getEpisodes(webPage, matching, website);
                    }
                    catch
                    {
                        matching.Completed = false;
                    }

                    return matching;
                }
            }

            return null;
        }

        private async Task<List<EpisodeMatching>> getEpisodes(Page webPage, AnimeMatching matching, Website website)
        {
            List<EpisodeMatching> episodes = new List<EpisodeMatching>();

            string url = $"{website.SiteUrl}{matching.Path.Replace(website.SiteUrl, string.Empty)}";
            await webPage.GoToAsync(url);

            await webPage.WaitForSelectorAsync(website.EpisodesSearch.WaitSelector, new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 3000
            });

            await webPage.ScreenshotAsync("episodes.png");

            ElementHandle description = await webPage.QuerySelectorAsync(website.EpisodesSearch.DescriptionSelector);
            matching.Description = (await description.EvaluateFunctionAsync<string>($"e => {website.EpisodesSearch.DescriptionFunction}")).Trim();

            if (website.ScrapeType == WebsiteScrapeType.EPISODES_DIFFERENT_PAGE)
            {
                int number = 1;
                foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(website.EpisodesSearch.ElementsSelector))
                {
                    EpisodeMatching episode = new EpisodeMatching()
                    {
                        Number = number
                    };

                    ElementHandle info = await element.QuerySelectorAsync(website.EpisodesSearch.InfoSelector);

                    episode.Path = await info.EvaluateFunctionAsync<string>($"e => {website.EpisodesSearch.PathFunction}");

                    if (!string.IsNullOrEmpty(episode.Path))
                    {
                        episode.Path = episode.Path.Trim();
                        episode.Title = (await info.EvaluateFunctionAsync<string>($"e => {website.EpisodesSearch.TitleFunction}")).Trim();
                    }

                    if (!string.IsNullOrEmpty(episode.Title))
                    {
                        episodes.Add(episode);
                    }

                    number++;
                }
            }
            else if(website.ScrapeType == WebsiteScrapeType.EPISODES_SAME_PAGE)
            {
                int number = 1;

                ElementHandle check = null;

                if (!string.IsNullOrEmpty(website.EpisodesSearch.PagesSelector))
                {
                    check = await webPage.QuerySelectorAsync(website.EpisodesSearch.PagesSelector);
                }

                if (check != null)
                {
                    foreach(ElementHandle page in await webPage.QuerySelectorAllAsync(website.EpisodesSearch.PagesSelector))
                    {
                        await page.ClickAsync();

                        foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(website.EpisodesSearch.ElementsSelector))
                        {
                            EpisodeMatching episode = await this.getEpisodeFromSamePage(webPage, element, number, website);

                            if (!string.IsNullOrEmpty(episode.Title))
                            {
                                episodes.Add(episode);
                            }

                            number++;
                        }
                    }
                }
                else 
                {
                    foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(website.EpisodesSearch.ElementsSelector))
                    {
                        EpisodeMatching episode = await this.getEpisodeFromSamePage(webPage, element, number, website);

                        if (!string.IsNullOrEmpty(episode.Title))
                        {
                            episodes.Add(episode);
                        }

                        number++;
                    }
                }
            }

            return episodes;
        }

        private async Task<EpisodeMatching> getEpisodeFromSamePage(Page webPage, ElementHandle element, int number, Website website)
        {
            EpisodeMatching episode = new EpisodeMatching()
            {
                Number = number
            };

            await element.ClickAsync();

            await webPage.WaitForSelectorAsync(website.EpisodesSource.WaitSelector, new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 3000
            });

            ElementHandle info = await webPage.QuerySelectorAsync(website.EpisodesSearch.InfoSelector);
            episode.Title = (await info.EvaluateFunctionAsync<string>($"e => {website.EpisodesSearch.TitleFunction}")).Trim();
            episode.Path = webPage.Url;

            ElementHandle video = await webPage.QuerySelectorAsync(website.EpisodesSource.SourceSelector);
            episode.Source = (await video.EvaluateFunctionAsync<string>($"e => {website.EpisodesSource.SourceFunction}")).Trim();

            return episode;
        }

        private async Task<string> getEpisodeSource(Page webPage, EpisodeMatching episode, Website website)
        {
            string source = null;
            string url = $"{website.SiteUrl}{episode.Path}";
            await webPage.GoToAsync(url);

            await webPage.WaitForSelectorAsync(website.EpisodesSource.WaitSelector, new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 3000
            });
            
            await webPage.ScreenshotAsync("source.png");

            if (website.EpisodesSource.HasQuality)
            {
                int maxQuality = 0;

                foreach(ElementHandle element in await webPage.QuerySelectorAllAsync(website.EpisodesSource.SourceSelector))
                {
                    string quality = (await element.EvaluateFunctionAsync<string>($"e => {website.EpisodesSource.QualityFunction}")).Trim();

                    if (!string.IsNullOrEmpty(website.EpisodesSource.QualityReplace))
                    {
                        quality = quality.Replace(website.EpisodesSource.QualityReplace, string.Empty);
                    }

                    int q = Convert.ToInt32(quality);

                    if(q > maxQuality)
                    {
                        maxQuality = q;
                        source = (await element.EvaluateFunctionAsync<string>($"e => {website.EpisodesSource.SourceFunction}")).Trim();
                    }
                }
            }
            else
            {
                ElementHandle video = await webPage.QuerySelectorAsync(website.EpisodesSource.SourceSelector);
                source = (await video.EvaluateFunctionAsync<string>($"e => {website.EpisodesSource.SourceFunction}")).Trim();
            }

            return source;
        }
        */
    }
}
