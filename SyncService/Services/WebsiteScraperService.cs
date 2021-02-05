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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class WebsiteScraperService : IService
    {
        #region Members

        private WebsiteCollection _websiteCollection = new WebsiteCollection();
        private AnimeCollection _animeCollection = new AnimeCollection();
        private long _lastId = -1;

        protected override int TimeToWait => 1000 * 10;

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("WebsiteScraper");
        }

        public override async void Start()
        {
            this._lastId = this._animeCollection.Last().Id;

            base.Start();
        }

        public override async void Work()
        {
            base.Work();

            try
            {
                Paging<Website> websites = this._websiteCollection.GetList(new WebsiteFilter());

                foreach(Website website in websites.Documents)
                {
                    try
                    {
                        for (int id = 1; id < this._lastId; id++)
                        {
                            Anime anime = this._animeCollection.Get(id);

                            using (Page webPage = await ProxyHelper.Instance.GetBestProxy())
                            {
                                string matching = await this.getMatchings(webPage, anime, website);

                                if (matching != null)
                                {
                                    this.Log($"Matched \"{anime.Titles[LocalizationEnum.English]}\" with \"{matching}\"");
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                this.Stop();
            }
            
            this.Wait();
        }

        private async Task<string> getMatchings(Page webPage, Anime anime, Website website)
        {
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
                ElementHandle title = await element.QuerySelectorAsync(website.Search.TitleSelector);
                string elementTitle = (await title.EvaluateFunctionAsync<string>($"e => {website.Search.TitleFunction}")).Trim();

                int ratio = Fuzz.TokenSortRatio(elementTitle, animeTitle);

                if(ratio == 100)
                {
                    return elementTitle;
                }
            }

            return null;
        }

        //string animeTitle = "THE GOD OF HIGH SCHOOL";
        //int episodes = 960;
        //bool found = false;
        //
        //string match = string.Empty;
        //List<string> possibleMatches = new List<string>();
        //
        //IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        //
        //string url = $"https://dreamsub.stream/search/?q={Uri.EscapeUriString(animeTitle)}";
        //var doc = await context.OpenAsync(url);
        //
        //IHtmlCollection<IElement> results = doc.QuerySelectorAll("#main-content .goblock .tvBlock");
        //
        //foreach (var r in results)
        //{
        //    string title = r.QuerySelector(".tvTitle .title").TextContent;
        //
        //    int ratio = Fuzz.TokenSortRatio(title, animeTitle);
        //
        //    if(ratio == 100)
        //    {
        //        found = true;
        //        match = title;
        //    }
        //    else if(found == false)
        //    {
        //        possibleMatches.Add(title);
        //    }
        //
        //    this.Log($"{title} => {ratio}");
        //IElement link = r.QuerySelector(".showStreaming a");
        //string episodesUrl = link.GetAttribute("href");
        //
        //if (title == animeTitle)
        //{
        //    this.Log($"TITLE: {title}, URL: {episodesUrl}");
        //
        //    for(int i = 1; i <= episodes; i++)
        //    {
        //        try
        //        {
        //            url = $"https://dreamsub.stream{episodesUrl}/{i}";
        //            doc = await context.OpenAsync(url);
        //
        //            var video = doc.QuerySelector("#media-play #iFrameVideoSub");
        //            string videoUrl = video.GetAttribute("src");
        //x
        //            this.Log($"VIDEO: {videoUrl}");
        //        }
        //        catch(Exception ex)
        //        {
        //            this.Log($"ERROR: {ex.Message}");
        //        }
        //    }
        //
        //
        //}
        //}
    }
}
