using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using FuzzySharp;
using MongoService;
using PuppeteerSharp;
using SyncService.Helpers;
using SyncService.Models;
using SyncService.Models.WebsiteScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class WebsiteScraperService : IService
    {
        #region Members

        private List<IWebsiteScraper> _workers;

        protected override int TimeToWait => 1000 * 60 * 10; // 10 Minutes

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("WebsiteScraper");
        }

        public override async Task Work()
        {
            await base.Work();

            try
            {
                this._workers = new List<IWebsiteScraper>()
                {
                    new DreamsubScraper(this),
                    new AnimeworldScraper(this),
                    new GoganimeScraper(this)
                };

                foreach(IWebsiteScraper scraper in this._workers)
                {
                    await scraper.Start();
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
