using FuzzySharp;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models.WebsiteScrapers
{
    public class AnimeworldScraper : IWebsiteScraper
    {
        public AnimeworldScraper(WebsiteScraperService service) : base(service)
        {
        }

        protected override long WebsiteID => 2;

        protected override Type WebsiteType => typeof(AnimeworldScraper);

        private List<EpisodeMatching> episodesMatchings = new List<EpisodeMatching>();

        protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
        {
            AnimeMatching matching = null;

            string url = $"{this.Website.SiteUrl}search?keyword={animeTitle}";
            await ProxyHelper.NavigateAsync(webPage, url);

            episodesMatchings.Clear();

            await webPage.WaitForSelectorAsync(".film-list", new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 2000
            });

            foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(".film-list .item"))
            {
                matching = new AnimeMatching();

                ElementHandle title = await element.QuerySelectorAsync(".inner .name");
                matching.Title = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                if (this.AnalyzeMatching(matching, animeTitle))
                {
                    // Setting Up URL
                    ElementHandle path = await element.QuerySelectorAsync(".inner .name");
                    matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();
                    
                    return matching;
                }
            }

            return null;
        }

        protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
        {
            string url;

            if (episodesMatchings.Count == 0)
            {
                url = this.Website.SiteUrl.Substring(0, this.Website.SiteUrl.Length - 1);
                url = $"{url}{matching.Path}";
                await ProxyHelper.NavigateAsync(webPage, url);

                await webPage.WaitForSelectorAsync(".widget.servers", new WaitForSelectorOptions()
                {
                    Visible = true,
                    Timeout = 2000
                });

                // Description
                ElementHandle description = await webPage.QuerySelectorAsync(".long");
                matching.Description = (await description.EvaluateFunctionAsync<string>("e => e.innerHTML")).Trim();

                // Get the Active Server (It will be always the Main server "AnimeWorld")
                string serverID = await webPage.QuerySelectorAsync(".tab.server-tab.active")
                    .EvaluateFunctionAsync<string>("e => e.getAttribute('data-name')");

                foreach (ElementHandle server in await webPage.QuerySelectorAllAsync(".server"))
                {
                    string sID = await server.EvaluateFunctionAsync<string>("e => e.getAttribute('data-name')");
                    if (serverID == sID)
                    {
                        foreach (ElementHandle ep in await server.QuerySelectorAllAsync("ul.episodes.range li.episode"))
                        {
                            ElementHandle info = await ep.QuerySelectorAsync("a");

                            string path = await info.EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
                            string title = (await info.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                            episodesMatchings.Add(new EpisodeMatching()
                            {
                                Path = path,
                                Title = title
                            });
                        }
                        break;
                    }
                }
            }

            EpisodeMatching episode = episodesMatchings[number - 1];

            if (episode != null)
            {
                episode.Number = number;

                if (!string.IsNullOrEmpty(episode.Path))
                {
                    episode.Path = episode.Path.Trim();
                    episode.Title = "";

                    url = this.Website.SiteUrl.Substring(0, this.Website.SiteUrl.Length - 1);
                    url = $"{url}{episode.Path}";
                    var watch = Stopwatch.StartNew();

                    await ProxyHelper.NavigateAsync(webPage, url);

                    watch.Stop();

                    await webPage.WaitForSelectorAsync("#download", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });

                    ElementHandle tempSource = await webPage.QuerySelectorAsync("#download #alternativeDownloadLink");
                    episode.Source = await tempSource
                        .EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
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
