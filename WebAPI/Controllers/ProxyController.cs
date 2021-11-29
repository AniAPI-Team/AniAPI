using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebAPI.Controllers
{
    [ApiVersion("1")]
    [Route("proxy")]
    [ApiController]
    /// <summary>
    /// Controller for Episode's video requests
    /// </summary>
    public class ProxyController : Controller
    {
        private readonly ILogger<ProxyController> _logger;
        private WebsiteCollection _websiteCollection = new WebsiteCollection();

        public ProxyController(ILogger<ProxyController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("{url}/{websiteName}"), MapToApiVersion("1")]
        [HttpGet("{url}/{websiteName}/{segment}"), MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task GetFormattedEpisodeSourceURL(string url, string websiteName, [FromQuery] Dictionary<string, string> values, string segment = null)
        {
            try
            {
                HttpProxyOptions options = null;
                
                url = HttpUtility.UrlDecode(url);

                switch (websiteName)
                {
                    case "dreamsub":
                        options = HttpProxyOptionsBuilder.Instance
                            .WithHttpClientName("HttpClientWithSSLUntrusted")
                            .WithShouldAddForwardedHeaders(false)
                            .WithBeforeSend((context, request) =>
                            {
                                request.Headers.Host = values["host"];
                                request.Headers.Referrer = new Uri(values["referrer"]);

                                return Task.CompletedTask;
                            })
                            .Build();
                        break;
                    case "gogoanime":
                        url = getGogoanimeURL(url);

                        options = HttpProxyOptionsBuilder.Instance
                            .WithHttpClientName("HttpClientWithSSLUntrusted")
                            .WithShouldAddForwardedHeaders(false)
                            .WithBeforeSend((context, request) =>
                            {
                                request.Headers.Referrer = new Uri(values["referrer"]);

                                return Task.CompletedTask;
                            })
                            .Build();
                        break;
                }

                // 15.09.2021: Added support for M3U8 streaming
                if (!string.IsNullOrEmpty(segment))
                {
                    string[] urlParts = url.Split('/');
                    urlParts[urlParts.Length - 1] = segment;

                    url = String.Join('/', urlParts);
                }

                if (options == null)
                {
                    throw new Exception($"Website {websiteName} not supported!");
                }
             
                return this.HttpProxyAsync(url, options);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private string getGogoanimeURL(string url)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage response = client.SendAsync(request).Result;

                string res = response.Content.ReadAsStringAsync().Result;

                Regex rgx = new Regex(@"<div class=""dowload""><a\s*href=""https://gogo-cdn.com(.*).*"".*\s*[(](\d*)P.*</div>", RegexOptions.None);
                MatchCollection matches = rgx.Matches(res);

                if (!matches.Any())
                {
                    throw new Exception("Video URLs not found!");
                }

                int maxQ = 0;
                string dUrl = string.Empty;
                foreach(Match match in matches)
                {
                    int q = Convert.ToInt32(match.Groups[2].Value);
                    
                    if (q > maxQ)
                    {
                        dUrl = $"https://gogo-cdn.com{match.Groups[1].Value}";
                        maxQ = q;
                    }
                }

                return dUrl;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
