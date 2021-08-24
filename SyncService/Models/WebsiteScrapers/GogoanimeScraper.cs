using Commons;
using Commons.Collections;
using FuzzySharp;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncService.Models.WebsiteScrapers
{
    public class GogoanimeScraper : IWebsiteScraper
    {

        public GogoanimeScraper(WebsiteScraperService service) : base(service)
        {
        }

        protected override long WebsiteID => 3;

        protected override Type WebsiteType => typeof(GogoanimeScraper);

        protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
        {
            AnimeMatching matching = null;

            string url = $"{this.Website.SiteUrl}search.html?keyword={animeTitle}";
            await ProxyHelper.NavigateAsync(webPage, url);

            Uri oldUri = new Uri(url);
            Uri pageUri = new Uri(webPage.Url);

            if(oldUri.Host != pageUri.Host)
            {
                this.Website.SiteUrl = $"https://{pageUri.Host}/";

                Website website = this.Website;
                new WebsiteCollection().Edit(ref website);
            }

            EpisodeMatchings.Clear();

            await webPage.WaitForSelectorAsync(".last_episodes", new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 2000
            });

            foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(".items li"))
            {
                matching = new AnimeMatching();

                ElementHandle title = await element.QuerySelectorAsync(".name");
                matching.Title = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                if (this.AnalyzeMatching(matching, animeTitle))
                {
                    // Setting Up URL
                    ElementHandle path = await title.QuerySelectorAsync("a");
                    matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                    matching.Linked = new AnimeMatching()
                    {
                        Title = matching.Title,
                        Path = $"{matching.Path}-dub",
                        SourceVariant = "_dub"
                    };

                    return matching;
                }
            }

            return null;
        }

        protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
        {
            string url;

            if (EpisodeMatchings.Count == 0)
            {
                url = this.Website.SiteUrl.Substring(0, this.Website.SiteUrl.Length - 1);
                url = $"{url}{matching.Path}";

                await ProxyHelper.NavigateAsync(webPage, url);

                await webPage.WaitForSelectorAsync(".anime_info_episodes_next", new WaitForSelectorOptions()
                {
                    Visible = true,
                    Timeout = 2000
                });

                // Anime ID per la chiamata AJAX
                ElementHandle animeIDElement = await webPage.QuerySelectorAsync(".anime_info_episodes_next .movie_id");
                string animeID = await animeIDElement.EvaluateFunctionAsync<string>("e => e.getAttribute('value')");

                // TODO: Verificare che i titoli per la richiesta siano effettivamente uguali a quelli visualizzati
                //string ajaxTitle = matching.Title.Replace(" ", "-").ToLower();

                // Custom URL that will use ajax call
                string ajax_url =
                    string.Format("https://ajax.gogo-load.com/ajax/load-list-episode?ep_start=0&ep_end=9999&id={0}" +
                    "&default_ep=0", animeID);

                // Get Ajax request Page
                await ProxyHelper.NavigateAsync(webPage, ajax_url);

                // Description
                matching.Description = ""; // TODO: Aggiungere descrizione

                foreach (ElementHandle ep in await webPage.QuerySelectorAllAsync("#episode_related li"))
                {
                    ElementHandle info = await ep.QuerySelectorAsync("a");

                    string path = await info.EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
                    string title = (await info.QuerySelectorAsync(".name").EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                    EpisodeMatchings.Add(new EpisodeMatching()
                    {
                        Path = path,
                        Title = title
                    });
                }
                
                EpisodeMatchings.Reverse();
            }

            EpisodeMatching episode = EpisodeMatchings[number - 1];

            if (episode != null)
            {
                episode.Number = number;

                if (!string.IsNullOrEmpty(episode.Path))
                {
                    episode.Path = episode.Path.Trim();
                    episode.Title = "";

                    var watch = Stopwatch.StartNew();

                    url = this.Website.SiteUrl.Substring(0, this.Website.SiteUrl.Length - 1);
                    url = $"{url}{episode.Path}";

                    await ProxyHelper.NavigateAsync(webPage, url);

                    watch.Stop();

                    // 24.08.2021: Needed a further step to avoid token related problems
                    /*
                    await webPage.WaitForSelectorAsync(".vidcdn", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });
                    
                    ElementHandle tempSource = await webPage.QuerySelectorAsync(".vidcdn a");
                    string videoPageUrl = await tempSource
                        .EvaluateFunctionAsync<string>("e => e.getAttribute('data-video')");
                    
                    if (!videoPageUrl.Contains("https:"))
                        videoPageUrl = $"https:{videoPageUrl}";
                    
                    episode.Source = BuildAPIProxyURL(videoPageUrl);
                    */

                    await webPage.WaitForSelectorAsync(".play-video", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });

                    ElementHandle tempSource = await webPage.QuerySelectorAsync(".play-video iframe");
                    string streamaniUrl = await tempSource.EvaluateFunctionAsync<string>("e => e.getAttribute('src')");

                    if (!streamaniUrl.Contains("https:"))
                    {
                        streamaniUrl = $"https:{streamaniUrl}";
                    }

                    episode.Source = BuildAPIProxyURL(streamaniUrl);
                }

                if (!string.IsNullOrEmpty(episode.Source))
                {
                    return episode;
                }
            }

            return null;
        }
    }
}
