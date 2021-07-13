using Commons;
using Commons.Collections;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Helpers
{
    public class ProxyHelper
    {
        #region Members

        private AppSettings _appSettings;
        private Dictionary<int, long> _proxies = new Dictionary<int, long>();
        bool _needDownload = true;

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

        public static async Task NavigateAsync(Page page, string url)
        {
            try
            {
                int timeout = 1000 * 15;
                WaitUntilNavigation[] waitUntil = new WaitUntilNavigation[]
                {
                    WaitUntilNavigation.Load,
                    WaitUntilNavigation.DOMContentLoaded
                };

                await page.GoToAsync(url, timeout, waitUntil);
            }
            catch
            {
                throw;
            }
        }

        private ProxyHelper()
        {
            this._appSettings = new AppSettingsCollection().Get(0);

            for(int i = 1; i <= this._appSettings.ProxyCount; i++)
            {
                this._proxies[i] = 0;
            }
        }

        public async Task<Browser> GetBrowser()
        {
            try
            {
                if (_needDownload)
                {
                    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                    _needDownload = false;
                }

#if DEBUG
                Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = false,
                    Args = new string[]
                        {
                            "--incognito",
                            $"--proxy-server={this._appSettings.ProxyHost}:{this._appSettings.ProxyPort}"
                        },
                    IgnoreHTTPSErrors = true
                });
#else
                Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    Args = new string[]
                        {
                            "--incognito",
                            $"--proxy-server={this._appSettings.ProxyHost}:{this._appSettings.ProxyPort}"
                        },
                    IgnoreHTTPSErrors = true
                });
#endif

                return browser;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Page> GetBestProxy (Browser browser, bool canBlockRequests)
        {
            try
            {
                int user = -1;
                bool needReset = false;
                long best = long.MaxValue;

                foreach (int key in this._proxies.Keys)
                {
                    long proxyUses = this._proxies[key];

                    if (proxyUses == long.MaxValue)
                    {
                        needReset = true;
                    }

                    if (proxyUses < best)
                    {
                        best = proxyUses;
                        user = key;
                    }
                }

                if (needReset)
                {
                    foreach (int key in this._proxies.Keys)
                    {
                        this._proxies[key] = 0;
                    }
                }

                this._proxies[user]++;

                Page webPage = (await browser.PagesAsync())[0];

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

                string proxyUsername = $"{this._appSettings.ProxyUsername}{user}";
                await webPage.AuthenticateAsync(new Credentials()
                {
                    Username = proxyUsername,
                    Password = this._appSettings.ProxyPassword
                });

                await webPage.Client.SendAsync("Network.setBlockedURLs", new Dictionary<string, object>
                {
                    ["urls"] = new string[]
                    {
                        "*.jpg", "*.png", "*.gif", "*.svg",
                        "*.mp4", "*.avi", "*.flv", "*.mov", "*.wmv",
                    }
                }).ConfigureAwait(false);

                webPage.Response += WebPage_Response;

                return webPage;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private async void WebPage_Response(object sender, ResponseCreatedEventArgs e)
        {
            Page page = (Page)sender;
            HttpResponseMessage response = new HttpResponseMessage(e.Response.Status);

            if(page.MainFrame.Url == "about:blank")
            {
                if (!response.IsSuccessStatusCode)
                {
#if DEBUG
                    Console.WriteLine(e.Response.Status.ToString());
#endif
                }
            }
        }

        private async void WebPage_Request(object sender, RequestEventArgs e)
        {
            List<string> imageFormats = new List<string>()
            {
                ".jpg", 
                ".png", 
                ".gif", 
                ".svg"
            };
            List<string> videoFormats = new List<string>()
            {
                ".mp4", 
                ".avi", 
                ".flv", 
                ".mov", 
                ".wmv"
            };

            if (e.Request.ResourceType == ResourceType.Image || e.Request.ResourceType == ResourceType.Media || e.Request.ResourceType == ResourceType.Img || e.Request.ResourceType == ResourceType.StyleSheet || e.Request.ResourceType == ResourceType.Font)
            {
                await e.Request.AbortAsync();
                return;
            }
            else if (e.Request.Url.ToLower().Contains("google") || 
                e.Request.Url.ToLower().Contains("ads") || 
                e.Request.Url.ToLower().Contains("antiadblocksystems") || 
                e.Request.Url.ToLower().Contains("stats") ||
                e.Request.Url.ToLower().Contains("run-syndicate"))
            {
                await e.Request.AbortAsync();
                return;
            }

            string requestFormat = e.Request.Url.Split('.').Last().Split('?')[0];

            if (!string.IsNullOrEmpty(requestFormat))
            {
                if (imageFormats.Contains($".{requestFormat}") ||
                    videoFormats.Contains($".{requestFormat}"))
                {
                    await e.Request.AbortAsync();
                    return;
                }
            }

            await e.Request.ContinueAsync();
        }

        #endregion
    }
}
