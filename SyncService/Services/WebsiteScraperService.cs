using AngleSharp;
using AngleSharp.Dom;
using Commons;
using Commons.Enums;
using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Services
{
    public class WebsiteScraperService : IService
    {
        protected override int TimeToWait => 1000 * 10;

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("WebsiteScraper");
        }

        public override async void Work()
        {
            base.Work();

            string animeTitle = "THE GOD OF HIGH SCHOOL";
            int episodes = 960;
            bool found = false;

            string match = string.Empty;
            List<string> possibleMatches = new List<string>();

            IBrowsingContext context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

            string url = $"https://dreamsub.stream/search/?q={Uri.EscapeUriString(animeTitle)}";
            var doc = await context.OpenAsync(url);

            IHtmlCollection<IElement> results = doc.QuerySelectorAll("#main-content .goblock .tvBlock");

            foreach (var r in results)
            {
                string title = r.QuerySelector(".tvTitle .title").TextContent;

                int ratio = Fuzz.TokenSortRatio(title, animeTitle);

                if(ratio == 100)
                {
                    found = true;
                    match = title;
                }
                else if(found == false)
                {
                    possibleMatches.Add(title);
                }

                this.Log($"{title} => {ratio}");
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
            }
            
            this.Wait();
        }
    }
}
