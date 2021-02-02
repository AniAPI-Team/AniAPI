using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string ip = httpContext.Connection.RemoteIpAddress.ToString();
            string method = httpContext.Request.Method;

            string protocol = httpContext.Request.IsHttps ? "https" : "http";
            string host = httpContext.Request.Host.Host;
            string port = httpContext.Request.Host.Port.ToString();
            string path = httpContext.Request.Path.ToString();
            string query = string.Empty;

            foreach(string key in httpContext.Request.Query.Keys)
            {
                query += $"{key}={Uri.EscapeUriString(httpContext.Request.Query[key])}";

                if(httpContext.Request.Query[key] != httpContext.Request.Query.Last().Value)
                {
                    query += "&";
                }
            }

            if (!string.IsNullOrEmpty(query))
            {
                query = $"?{query}";
            }

            string uri = $"{protocol}://{host}:{port}{path}{query}";

            _logger.LogInformation($"{method} {uri} FROM {ip}");

            await _next(httpContext);
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
