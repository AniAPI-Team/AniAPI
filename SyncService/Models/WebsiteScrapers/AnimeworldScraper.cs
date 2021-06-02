using FuzzySharp;
using PuppeteerSharp;
using SyncService.Services;
using System;
using System.Collections.Generic;
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

        protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
        {
            throw new NotImplementedException();
        }

        protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
        {
            AnimeMatching matching = null;

            string url = $"{this.Website.SiteUrl}search?keyword={animeTitle}";
            await webPage.GoToAsync(url);

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
                Console.WriteLine($"Finding: { matching.Title }");

                if (this.AnalyzeMatching(matching, animeTitle))
                {
                    this.Service.Log($"Found: { matching.Title }");

                    // Setting Up URL
                    ElementHandle path = await element.QuerySelectorAsync(".inner .name");
                    matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                    this.Service.Log($"URL: { matching.Path }");
                    return matching;
                }
            }

            return null;
        }
    }
}
