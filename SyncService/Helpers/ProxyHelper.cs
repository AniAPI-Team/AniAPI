using Commons;
using Commons.Collections;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Helpers
{
    public class ProxyHelper
    {
        #region Members

        private AppSettings _appSettings;
        private Dictionary<int, long> _proxies = new Dictionary<int, long>();
        private Browser _puppeteerClient;

        #endregion

        #region Singleton

        private static ProxyHelper _instance;
        public static ProxyHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProxyHelper();
                }

                return _instance;
            }
        }

        private ProxyHelper()
        {
            this._appSettings = new AppSettingsCollection().Get(0);

            for(int i = 1; i <= this._appSettings.ProxyCount; i++)
            {
                this._proxies[i] = 0;
            }

            new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision).Wait();
        }

        public async Task<Page> GetBestProxy()
        {
            if(this._puppeteerClient != null && !this._puppeteerClient.IsClosed)
            {
                await this._puppeteerClient.CloseAsync();
            }

            int user = -1;
            bool needReset = false;
            long best = long.MaxValue;

            foreach(int key in this._proxies.Keys)
            {
                long proxyUses = this._proxies[key];

                if(proxyUses == long.MaxValue)
                {
                    needReset = true;
                }

                if(proxyUses < best)
                {
                    best = proxyUses;
                    user = key;
                }
            }

            if (needReset)
            {
                foreach(int key in this._proxies.Keys)
                {
                    this._proxies[key] = 0;
                }
            }

            this._proxies[user]++;

            this._puppeteerClient = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true,
                Args = new string[]
                {
                    $"--proxy-server={this._appSettings.ProxyHost}:{this._appSettings.ProxyPort}"
                }
            });

            Page webPage = await this._puppeteerClient.NewPageAsync();
            await webPage.AuthenticateAsync(new Credentials()
            {
                Username = $"{this._appSettings.ProxyUsername}{user}",
                Password = this._appSettings.ProxyPassword
            });

            return webPage;
        }

        #endregion
    }
}
