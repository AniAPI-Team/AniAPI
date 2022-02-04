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

            await webPage.WaitForSelectorAsync("ul.listing.items", new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 2000
            });

            foreach (ElementHandle element in await webPage.QuerySelectorAllAsync("ul.listing.items li.video-block"))
            {
                matching = new AnimeMatching();

                ElementHandle image = await element.QuerySelectorAsync(".img .picture img");
                matching.Title = (await image.EvaluateFunctionAsync<string>("e => e.getAttribute('alt')")).Trim();

                //ElementHandle title = await element.QuerySelectorAsync(".name");
                //matching.Title = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                ElementHandle path = await element.QuerySelectorAsync("a");
                matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                if (this.AnalyzeMatching(matching, animeTitle))
                {
                    string[] pathParts = matching.Path.Split("-episode-");

                    matching.Linked = new AnimeMatching()
                    {
                        Title = matching.Title,
                        Path = $"{pathParts[0]}-dub-episode-{pathParts[1]}",
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

                await webPage.WaitForSelectorAsync(".video-details", new WaitForSelectorOptions()
                {
                    Visible = true,
                    Timeout = 2000
                });

                if (string.IsNullOrEmpty(matching.SourceVariant))
                {
                    ElementHandle detail = await webPage.QuerySelectorAsync(".video-details #rmjs-1 p:first-child");
                    matching.Description = (await detail.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();
                }

                int lastNumber = Convert.ToInt32(matching.Path.Split('-').Last());

                for(int i = 1; i <= lastNumber; i++)
                {
                    EpisodeMatchings.Add(new EpisodeMatching
                    {
                        Path = matching.Path.Replace(lastNumber.ToString(), i.ToString()),
                        Title = $"Episode {i}"
                    });
                }
            }

            EpisodeMatching episode = EpisodeMatchings[number - 1];

            if (episode != null)
            {
                episode.Number = number;

                if (!string.IsNullOrEmpty(episode.Path))
                {
                    episode.Path = episode.Path.Trim();

                    url = this.Website.SiteUrl.Substring(0, this.Website.SiteUrl.Length - 1);
                    url = $"{url}{episode.Path}";

                    await ProxyHelper.NavigateAsync(webPage, url);

                    await webPage.WaitForSelectorAsync(".play-video iframe", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });

                    ElementHandle frame = await webPage.QuerySelectorAsync(".play-video iframe");
                    string src = (await frame.EvaluateFunctionAsync<string>("e => e.getAttribute('src')")).Trim();
                    
                    Regex rgx = new Regex(@"id=(.*?)&t", RegexOptions.None);
                    string id = rgx.Match(src).Groups[1].Value;
                    
                    url = $"https://gogoplay.link/api.php?id={id}";
                    
                    await ProxyHelper.NavigateAsync(webPage, url);

                    string text = (await webPage.QuerySelectorAsync("body pre").EvaluateFunctionAsync<string>("e => e.innerText")).Trim();
                    rgx = new Regex(@"m3u8.:.(.*?).}", RegexOptions.None);

                    string m3u8 = rgx.Match(text).Groups[1].Value.Replace("\\", string.Empty);

                    episode.Source = BuildAPIProxyURL(m3u8, new Dictionary<string, string>()
                    {
                        { "referrer", Website.SiteUrl }
                    });
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
