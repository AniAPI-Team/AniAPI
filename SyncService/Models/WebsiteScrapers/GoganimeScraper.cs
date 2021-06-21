using FuzzySharp;
using PuppeteerSharp;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncService.Models.WebsiteScrapers
{
    public class GoganimeScraper : IWebsiteScraper
    {

        public GoganimeScraper(WebsiteScraperService service) : base(service)
        {
        }

        protected override long WebsiteID => 3;

        protected override Type WebsiteType => typeof(GoganimeScraper);
        protected override bool WebsiteForceReload { get => true; }

        private List<EpisodeMatching> episodesMatchings = new List<EpisodeMatching>();

        protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
        {
            AnimeMatching matching = null;

            string url = $"{this.Website.SiteUrl}search.html?keyword={animeTitle}";
            await webPage.GoToAsync(url);

            episodesMatchings.Clear();

            await webPage.WaitForSelectorAsync(".last_episodes", new WaitForSelectorOptions()
            {
                Visible = true,
                Timeout = 2000
            });

            foreach (ElementHandle element in await webPage.QuerySelectorAllAsync(".items"))
            {
                matching = new AnimeMatching();

                ElementHandle title = await element.QuerySelectorAsync(".name");
                matching.Title = (await title.EvaluateFunctionAsync<string>("e => e.innerText")).Trim();
                //Console.WriteLine($"Finding: { matching.Title }");

                if (this.AnalyzeMatching(matching, animeTitle))
                {
                    //this.Service.Log($"Found: { matching.Title }");

                    // Setting Up URL
                    ElementHandle path = await title.QuerySelectorAsync("a");
                    matching.Path = (await path.EvaluateFunctionAsync<string>("e => e.getAttribute('href')")).Trim();

                    //this.Service.Log($"URL: { matching.Path }");
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

                //this.Service.Log(url);

                await webPage.GoToAsync(url);

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
                    string.Format("https://ajax.gogo-load.com/ajax/load-list-episode?ep_start=0&ep_end=500&id={0}" +
                    "&default_ep=0", animeID);

                // Get Ajax request Page
                await webPage.GoToAsync(ajax_url);

                // Description
                matching.Description = ""; // TODO: Aggiungere descrizione

                foreach (ElementHandle ep in await webPage.QuerySelectorAllAsync("#episode_related li"))
                {
                    ElementHandle info = await ep.QuerySelectorAsync("a");

                    string path = await info.EvaluateFunctionAsync<string>("e => e.getAttribute('href')");
                    string title = (await info.QuerySelectorAsync(".name").EvaluateFunctionAsync<string>("e => e.innerText")).Trim();

                    episodesMatchings.Add(new EpisodeMatching()
                    {
                        Path = path,
                        Title = title
                    });
                }
                episodesMatchings.Reverse();
            }

            EpisodeMatching episode = episodesMatchings[number - 1];

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

                    await webPage.GoToAsync(url);

                    watch.Stop();
                    //Console.WriteLine($"{episode.Path} - {number} - {watch.ElapsedMilliseconds}");

                    await webPage.WaitForSelectorAsync(".vidcdn", new WaitForSelectorOptions()
                    {
                        Visible = true,
                        Timeout = 2000
                    });

                    ElementHandle tempSource = await webPage.QuerySelectorAsync(".vidcdn a");
                    string videoPageUrl = await tempSource
                        .EvaluateFunctionAsync<string>("e => e.getAttribute('data-video')");

                    if (!videoPageUrl.Contains("https:"))
                        videoPageUrl = $"https:{ videoPageUrl }";

                    await webPage.GoToAsync(videoPageUrl);

                    // Get Page HTML Content As String
                    string pageContent = await webPage.GetContentAsync();

                    Regex rgx = new Regex(@"playerInstance\.setup\(\s*[^s]+sources\:\[\{file\: \'([^\']+)\'\,", RegexOptions.None);
                    Match match = rgx.Match(pageContent);

                    if (!match.Success)
                        return null;
                    else
                        episode.Source = match.Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(episode.Source))
                    return episode;
            }

            return null;
        }
    }
}
