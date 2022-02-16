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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebAPI.Controllers
{
    // Disabled on 16-02-2022
    // Actual VPS power is not enough in order to grant a great proxy service

    //[ApiVersion("1")]
    //[Route("proxy")]
    //[ApiController]
    //public class ProxyController : Controller
    //{
    //    private readonly ILogger<ProxyController> _logger;
    //    private WebsiteCollection _websiteCollection = new WebsiteCollection();

    //    public ProxyController(ILogger<ProxyController> logger)
    //    {
    //        _logger = logger;
    //    }

    //    [AllowAnonymous]
    //    [EnableCors("CorsEveryone")]
    //    [HttpGet("{url}")]
    //    [ApiExplorerSettings(IgnoreApi = true)]
    //    public Task ReverseProxy([FromRoute] string url, [FromQuery] Dictionary<string, string> values)
    //    {
    //        try
    //        {
    //            url = HttpUtility.UrlDecode(url).Replace(" ", "+");

    //            HttpProxyOptions options = HttpProxyOptionsBuilder.Instance
    //                .WithHttpClientName("HttpClientWithSSLUntrusted")
    //                .WithShouldAddForwardedHeaders(false)
    //                .WithBeforeSend((context, request) =>
    //                {
    //                    request.Headers.IfModifiedSince = null;

    //                    foreach (var h in request.Headers)
    //                    {
    //                        Console.WriteLine($"{h.Key}: {string.Join(',', h.Value?.Select(x => x))}");
    //                    }

    //                    if(values != null)
    //                    {
    //                        foreach (string k in values.Keys)
    //                        {
    //                            request.Headers.Remove(k);
    //                            request.Headers.Add(k, values[k]);
    //                        }
    //                    }

    //                    return Task.CompletedTask;
    //                })
    //                .WithAfterReceive(async (context, response) =>
    //                {
    //                    if (response.RequestMessage.RequestUri.PathAndQuery.Contains(".m3u8"))
    //                    {
    //                        string body = null;

    //                        using (var stream = await response.Content.ReadAsStreamAsync())
    //                        {
    //                            using(var decompressed = new GZipStream(stream, CompressionMode.Decompress))
    //                            {
    //                                using(var reader = new StreamReader(decompressed, Encoding.UTF8))
    //                                {
    //                                    body = await reader.ReadToEndAsync();
    //                                }
    //                            }
    //                        }

    //                        if (string.IsNullOrEmpty(body))
    //                        {
    //                            return;
    //                        }

    //                        string requestUrl = response.RequestMessage.RequestUri.AbsoluteUri;

    //                        if (requestUrl.EndsWith(".m3u8"))
    //                        {
    //                            var urlParts = requestUrl.Split('/');
    //                            requestUrl = string.Join('/', urlParts[0..(urlParts.Length - 1)]) + "/";
    //                        }

    //                        string proxiedBody = analyzeHLSBody(requestUrl, body, values);

    //                        response.Content = new StringContent(proxiedBody);
    //                    }
    //                })
    //                .Build();

    //            return this.HttpProxyAsync(url, options);
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    private string analyzeHLSBody(string requestUrl, string body, Dictionary<string, string> values = null)
    //    {
    //        string[] lines = body.Split('\n');

    //        for(int i = 0; i < lines.Length; i++)
    //        {
    //            if (lines[i].StartsWith("#"))
    //            {
    //                continue;
    //            }

    //            if (!lines[i].StartsWith("http"))
    //            {
    //                lines[i] = requestUrl + lines[i];
    //            }

    //            lines[i] = buildReversedProxyUrl(lines[i], values);
    //        }

    //        return string.Join('\n', lines);
    //    }

    //    private string buildReversedProxyUrl(string url, Dictionary<string, string> values)
    //    {
    //        url = $"/v1/proxy/{HttpUtility.UrlEncode(url)}/";

    //        if (values != null)
    //        {
    //            url += "?";
    //            List<string> queryParams = new List<string>();

    //            foreach (string k in values.Keys)
    //            {
    //                queryParams.Add($"{k}={HttpUtility.UrlEncode(values[k])}");
    //            }

    //            url += string.Join('&', queryParams);
    //        }

    //        return url;
    //    }
    //}
}