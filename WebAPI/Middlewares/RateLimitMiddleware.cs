using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Net;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Middlewares
{
    public class RateLimitMiddleware
    {
        private OAuthClientCollection _authClientCollection = new OAuthClientCollection();
        private readonly RequestDelegate _next;
        private readonly IRateLimitDependency _rateLimitDependency;

        public RateLimitMiddleware(RequestDelegate next, IRateLimitDependency rateLimitDependency)
        {
            _next = next;
            _rateLimitDependency = rateLimitDependency;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.ContainsKey("X-Client-Id"))
            {
                string clientId = httpContext.Request.Headers["X-Client-Id"].ToString();

                OAuthClient client = _authClientCollection.Collection.Find(x => x.ClientID == Guid.Parse(clientId) && x.IsUnlimited).FirstOrDefault();

                if(client != null)
                {
                    httpContext.Response.Headers.Add("X-RateLimit-Limit", "90");
                    httpContext.Response.Headers.Add("X-RateLimit-Remaining", "90");
                    httpContext.Response.Headers.Add("X-RateLimit-Reset", "0");

                    await _next(httpContext);
                    return;
                }
            }

            APIRequestIP requestIP = _rateLimitDependency.CanRequest(httpContext.Connection.RemoteIpAddress.ToString());

            httpContext.Response.Headers.Add("X-RateLimit-Limit", "90");

            int usesLeft = 90 - requestIP.Count;
            httpContext.Response.Headers.Add("X-RateLimit-Remaining", usesLeft.ToString());

            int secondsLeft = (int)(requestIP.FirstRequest.AddMinutes(1) - DateTime.Now).TotalSeconds;

            if(secondsLeft < 0)
            {
                secondsLeft = 0;
            }

            httpContext.Response.Headers.Add("X-RateLimit-Reset", secondsLeft.ToString());

            if (!requestIP.RateLimitOK)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

                APIResponse response = new APIResponse()
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                    Message = "Too many requests",
                    Data = "You reached the limit of 90 requests per minute"
                };

                await httpContext.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(httpContext);
        }
    }

    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}
