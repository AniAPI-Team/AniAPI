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
                        options = HttpProxyOptionsBuilder.Instance
                            .WithHttpClientName("HttpClientWithSSLUntrusted")
                            .WithShouldAddForwardedHeaders(false)
                            .WithBeforeSend((context, request) =>
                            {
                                if (values.Keys.Contains("referrer"))
                                {
                                    request.Headers.Referrer = new Uri(values["referrer"]);
                                }

                                return Task.CompletedTask;
                            })
                            .WithAfterReceive(async (context, response) =>
                            {
                                if (Url.IsLocalUrl(context.Connection.RemoteIpAddress.ToString()))
                                {
                                    return;
                                }

                                if (response.RequestMessage.RequestUri.PathAndQuery.Contains(".m3u8"))
                                {
                                    string body = await response.Content.ReadAsStringAsync();
                                    List<string> parsedBody = new List<string>();
                            
                                    foreach (string l in body.Split('\n').ToArray())
                                    {
                                        string parsedLine = l;
                            
                                        if (l.StartsWith("http"))
                                        {
                                            parsedLine = $"/v1/proxy/{HttpUtility.UrlEncode(l)}/gogoanime?referrer={HttpUtility.UrlEncode(values["referrer"])}";
                                        }
                                        else if (l.EndsWith(".m3u8"))
                                        {
                                            parsedLine = $"{l}?referrer={HttpUtility.UrlEncode(values["referrer"])}";
                                        }
                            
                                        parsedBody.Add(parsedLine);
                                    }
                            
                                    response.Content = new StringContent(string.Join('\n', parsedBody));
                                }
                            })
                            .Build();
                        break;
                }

                // 15.09.2021: Added support for HLS streaming
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
    }
}
