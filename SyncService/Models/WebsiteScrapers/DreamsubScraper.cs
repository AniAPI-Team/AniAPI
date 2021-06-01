using Commons;
using FuzzySharp;
using PuppeteerSharp;
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

            episodesMatchings.Clear();

            string url = $"{this.Website.SiteUrl}/search/?q={animeTitle}";
            await webPage.GoToAsync(url);

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

                matching.Score = Fuzz.TokenSortRatio(matching.Title, animeTitle);

                if (matching.Score == 100)
                {
                    ElementHandle path = await element.QuerySelectorAsync(".showStreaming a");
                    matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                    return matching;
                }
            }

            return null;
        }

        private List<EpisodeMatching> episodesMatchings = new List<EpisodeMatching>();

        protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
        {
            string url = string.Empty;

            if(episodesMatchings.Count == 0)
            {
                url = $"{this.Website.SiteUrl}{matching.Path}";
                await webPage.GoToAsync(url);

                await webPage.WaitForSelectorAsync("#episodes-list", new WaitForSelectorOptions()
                {
                    Visible = true,
                    Timeout = 2000
                });

                ElementHandle description = await webPage.QuerySelectorAsync("#tramaLong");
                matching.Description = (await description.EvaluateFunctionAsync<string>("e => e.innerHTML")).Trim();

                foreach(ElementHandle ep in await webPage.QuerySelectorAllAsync("#episodes-list .ep-item"))
                {
                    ElementHandle info = await ep.QuerySelectorAsync(".sli-name a");
                    
                    string path = await info.EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
                    string title = (await info.EvaluateFunctionAsync<string>("e => e.innerText")).Trim().Split(": ")[1];

                    episodesMatchings.Add(new EpisodeMatching()
                    {
                        Path = path,
                        Title = title
                    });
                }
            }

            EpisodeMatching episode = episodesMatchings[number - 1];

            if(episode != null)
            {
                episode.Number = number;

                if (!string.IsNullOrEmpty(episode.Path))
                {
                    episode.Path = episode.Path.Trim();
                    episode.Title = 

                    url = $"{this.Website.SiteUrl}{episode.Path}";
                    var watch = Stopwatch.StartNew();

                    await webPage.GoToAsync(url);

                    watch.Stop();
                    Console.WriteLine($"{number} - {watch.ElapsedMilliseconds}");

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
                            episode.Source = (await source.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();
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
