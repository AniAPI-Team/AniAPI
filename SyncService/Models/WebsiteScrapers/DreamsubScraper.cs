using Commons;
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
    public class DreamsubScraper : IWebsiteScraper
    {
        public DreamsubScraper(WebsiteScraperService service) : base(service)
        {
        }

        protected override long WebsiteID => 1;

        protected override Type WebsiteType => typeof(DreamsubScraper);

        protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
        {
            AnimeMatching matching = null;

            EpisodeMatchings.Clear();

            string url = $"{this.Website.SiteUrl}/search/?q={animeTitle}";
            await ProxyHelper.NavigateAsync(webPage, url);

            await webPage.WaitForSelectorAsync("#main-content", new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 2000
            });

            foreach (ElementHandle element in await webPage.QuerySelectorAllAsync("#main-content .tvBlock"))
            {
                matching = new AnimeMatching();

                ElementHandle title = await element.QuerySelectorAsync(".tvTitle .title");
                matching.Title = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                ElementHandle path = await element.QuerySelectorAsync(".showStreaming a");
                matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                if(this.AnalyzeMatching(matching, animeTitle))
                {
                    return matching;
                }
            }

            return null;
        }

        protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
        {
            string url;

            if(EpisodeMatchings.Count == 0)
            {
                url = $"{this.Website.SiteUrl}{matching.Path}";
                await ProxyHelper.NavigateAsync(webPage, url);

                await webPage.WaitForSelectorAsync("#episodes-list", new WaitForSelectorOptions()
                {
                    Visible = true,
                    Timeout = 2000
                });

                ElementHandle description = await webPage.QuerySelectorAsync("#tramaLong");
                matching.Description = (await description.EvaluateFunctionAsync<string>("e => e.innerHTML") ?? "").Trim();

                foreach(ElementHandle ep in await webPage.QuerySelectorAllAsync("#episodes-list .ep-item"))
                {
                    ElementHandle info = await ep.QuerySelectorAsync(".sli-name a");
                    
                    string path = await info.EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
                    string title = (await info.EvaluateFunctionAsync<string>("e => e.innerText")).Trim().Split(": ")[1];

                    EpisodeMatchings.Add(new EpisodeMatching()
                    {
                        Path = path,
                        Title = title
                    });
                }
            }

            EpisodeMatching episode = EpisodeMatchings[number - 1];

            if(episode != null)
            {
                episode.Number = number;

                if (!string.IsNullOrEmpty(episode.Path))
                {
                    episode.Path = episode.Path.Trim();

                    url = $"{this.Website.SiteUrl}{episode.Path}";

                    await ProxyHelper.NavigateAsync(webPage, url);

                    await webPage.WaitForSelectorAsync("#main-content.onlyDesktop", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });

                    int maxQuality = 0;

                    foreach (ElementHandle source in await webPage.QuerySelectorAllAsync("#main-content.onlyDesktop .goblock-content .dwButton"))
                    {
                        string quality = (await source.EvaluateFunctionAsync<string>("e => e.innerText")).Trim().Replace("p", string.Empty);
                        int q = Convert.ToInt32(quality);

                        if (q > maxQuality)
                        {
                            maxQuality = q;
                            string sourceUrl = (await source.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                            episode.Source = BuildAPIProxyURL(sourceUrl, new Dictionary<string, string>()
                            {
                                { "host", "cdn.dreamsub.cc" },
                                { "referrer", url }
                            });
                        }
                    }
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
