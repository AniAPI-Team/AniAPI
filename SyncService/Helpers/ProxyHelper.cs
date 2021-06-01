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
        private Dictionary<string, Browser> _browsers = new Dictionary<string, Browser>();

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

        public string GenerateBrowserKey(Type callerType)
        {
            return $"{callerType.Name}_{Guid.NewGuid()}";
        }

        public async Task<Page> GetBestProxy (string browserKey, bool canBlockRequests)
        {
            if (!this._browsers.Keys.Contains(browserKey))
            {
                this._browsers[browserKey] = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Args = new string[]
                    {
                        $"--proxy-server={this._appSettings.ProxyHost}:{this._appSettings.ProxyPort}"
                    }
                });
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

            Page webPage = (await this._browsers[browserKey].PagesAsync())[0];

            webPage = await this._browsers[browserKey].NewPageAsync();
            await webPage.SetCacheEnabledAsync(false);
            await webPage.SetViewportAsync(new ViewPortOptions()
            {
                Width = 1366,
                Height = 768
            });
            await webPage.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36'");

            if (canBlockRequests)
            {
                await webPage.SetRequestInterceptionAsync(true);
                webPage.Request += WebPage_Request;
            }

            await webPage.AuthenticateAsync(new Credentials()
            {
                Username = $"{this._appSettings.ProxyUsername}{user}",
                Password = this._appSettings.ProxyPassword
            });

            return webPage;
        }

        public async void CloseProxy(string key)
        {
            await this._browsers[key].CloseAsync();
        }

        private async void WebPage_Request(object sender, RequestEventArgs e)
        {
            if (e.Request.ResourceType == ResourceType.Image || e.Request.ResourceType == ResourceType.Media || e.Request.ResourceType == ResourceType.StyleSheet || e.Request.ResourceType == ResourceType.Font)
            {
                await e.Request.AbortAsync();
            }
            else if (e.Request.Url.ToLower().Contains("google") || 
                e.Request.Url.ToLower().Contains("ads") || 
                e.Request.Url.ToLower().Contains("antiadblocksystems") || 
                e.Request.Url.ToLower().Contains("stats"))
            {
                await e.Request.AbortAsync();
            }
            else
            {
                await e.Request.ContinueAsync();
            }
        }

        #endregion
    }
}
