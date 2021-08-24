﻿using AspNetCore.Proxy;
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task GetFormattedEpisodeSourceURL(string url, string websiteName, [FromQuery] Dictionary<string, string> values)
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
                        return new RedirectResult(url).ExecuteResultAsync(ControllerContext);
                }

                if(options == null)
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

                Regex rgx = new Regex(@"<li  class=""linkserver"" data-status=""1"" data-provider=""serverwithtoken""\s*[^s]+data-video=""([^\""]+)"">", RegexOptions.None);
                Match match = rgx.Match(res);

                if (!match.Success)
                {
                    throw new Exception("URL not found!");
                }

                string streamaniEmbedUrl = match.Groups[1].Value;

                request = new HttpRequestMessage(HttpMethod.Get, streamaniEmbedUrl);
                response = client.SendAsync(request).Result;

                res = response.Content.ReadAsStringAsync().Result;

                rgx = new Regex(@"playerInstance\.setup\(\s*[^s]+sources\:\[\{file\: \'([^\']+)\'\,", RegexOptions.None);
                match = rgx.Match(res);

                if (!match.Success)
                {
                    throw new Exception("URL not found!");
                }
                
                return match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
