using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class ApilyticsMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly RequestDelegate _next;

        public ApilyticsMiddleware(IConfiguration configuration, RequestDelegate next)
        {
            _configuration = configuration;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.StartsWith("/v1/proxy"))
            {
                await _next(httpContext);
                return;
            }

            DateTime executionStartTime = DateTime.Now;

            await _next(httpContext);

            double executionMs = DateTime.Now.Subtract(executionStartTime).TotalMilliseconds;

            var payload = new ApilyticsPayload
            {
                EndpointPath = httpContext.Request.Path,
                HttpMethod = httpContext.Request.Method,
                ExecutionTime = (long)executionMs,
                HttpStatusCode = httpContext.Response.StatusCode
            };

#if !DEBUG
            _ = Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://www.apilytics.io/api/v1/middleware");

                    client.DefaultRequestHeaders.Add("X-API-Key", _configuration.GetValue<string>("apilytics_key"));

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                    };

                    await client.SendAsync(request);
                }
            });
#endif
        }

        class ApilyticsPayload
        {
            [JsonProperty(PropertyName = "path")]
            public string EndpointPath { get; set; }

            [JsonProperty(PropertyName = "method")]
            public string HttpMethod { get; set; }

            [JsonProperty(PropertyName = "timeMillis")]
            public long ExecutionTime { get; set; }

            [JsonProperty(PropertyName = "statusCode")]
            public int HttpStatusCode { get; set; }
        }
    }

    public static class ApilyticsMiddlewareExtensions
    {
        public static IApplicationBuilder UseApyliticsMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApilyticsMiddleware>();
        }
    }
}
