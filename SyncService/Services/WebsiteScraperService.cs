using AngleSharp;
using Models;
using MongoService.Models;
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
            return new ServicesStatus()
            {
                Name = "WebsiteScraper",
                Status = ServiceStatusEnum.NONE
            };
        }

        public override async void Work()
        {
            base.Work();

            //var config = Configuration.Default.WithDefaultLoader();
            //var context = BrowsingContext.New(config);
            //var doc = await context.OpenAsync("https://dreamsub.stream/search/?q=ricerca");
            //
            //var results = doc.QuerySelectorAll("#main-content .goblock .tvBlock");
            //
            //foreach(var r in results)
            //{
            //    string title = r.QuerySelector(".showStreaming b:first-child").TextContent;
            //    this.Log(title);
            //}
            //
            //this.Wait();
        }
    }
}
