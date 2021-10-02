using Commons;
using Commons.Enums;
using GUI.Admin.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class CloudflareController : Controller
    {
        private readonly ILogger<CloudflareController> _logger;
        private readonly IConfiguration _configuration;

        private HttpClient _graphQLClient = new HttpClient() { BaseAddress = new Uri("https://api.cloudflare.com/client/v4/graphql") };

        public CloudflareController(ILogger<CloudflareController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public async Task<string> GetRequestMetrics()
        {
            string json = string.Empty;

            try
            {
                if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) == UserRoleEnum.BASIC.ToString())
                {
                    HttpContext.Response.StatusCode = 403;
                    return json;
                }

                DateTime today = DateTime.Now;
                DateTime yesterday = today.AddDays(-1);

                GraphQLQuery query = new GraphQLQuery()
                {
                    Query = @"
                        query ($zone_tag: string, $date_gt: Time, $date_lt: Time) {
                            viewer {
                                zones(filter: { zoneTag: $zone_tag }) {
                                    httpRequests1hGroups(
                                        filter: {
                                            datetime_gt: $date_gt,
                                            datetime_lt: $date_lt
                                        }, limit: 3 ) {
                                        sum {
                                            requests
                                            cachedRequests
                                        }
                                    }
                                }
                            }
                        }
                    ",
                    Variables = new Dictionary<string, object>()
                    {
                        { "zone_tag", _configuration.GetValue<string>("Cloudflare:ZoneId") },
                        { "date_gt", yesterday.ToString("yyyy-MM-ddTHH:mm:ssZ").Replace(".", ":") },
                        { "date_lt", today.ToString("yyyy-MM-ddTHH:mm:ssZ").Replace(".", ":") }
                    }
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("Cloudflare:Token"));

                using (var response = await _graphQLClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        throw;
                    }

                    json = await response.Content.ReadAsStringAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return json;
        }

        [Authorize]
        [HttpGet]
        public async Task<string> GetRequestChart()
        {
            string json = string.Empty;

            try
            {
                if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) == UserRoleEnum.BASIC.ToString())
                {
                    HttpContext.Response.StatusCode = 403;
                    return json;
                }

                DateTime lastMonth = DateTime.Now.AddMonths(-1);

                GraphQLQuery query = new GraphQLQuery()
                {
                    Query = @"
                        query ($zone_tag: string, $date: Date) {
                            viewer {
                                zones(filter: { zoneTag: $zone_tag }) {
                                    httpRequests1dGroups(
                                        filter: {
                                            date_gt: $date
                                        }, 
                                        limit: 1000,
                                        orderBy: [date_ASC]) {
                                        dimensions {
                                            date
                                        }
                                        sum {
                                            requests
                                            cachedRequests
                                        }
                                    }
                                }
                            }
                        }
                    ",
                    Variables = new Dictionary<string, object>()
                    {
                        { "zone_tag", _configuration.GetValue<string>("Cloudflare:ZoneId") },
                        { "date", lastMonth.ToString("yyyy-MM-dd") }
                    }
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("Cloudflare:Token"));

                using (var response = await _graphQLClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        throw;
                    }

                    json = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return json;
        }

        [Authorize]
        [HttpGet]
        public async Task<string> GetBandwidthMetrics()
        {
            string json = string.Empty;

            try
            {
                if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) == UserRoleEnum.BASIC.ToString())
                {
                    HttpContext.Response.StatusCode = 403;
                    return json;
                }

                DateTime today = DateTime.Now;
                DateTime yesterday = today.AddDays(-1);

                GraphQLQuery query = new GraphQLQuery()
                {
                    Query = @"
                        query ($zone_tag: string, $date_gt: Time, $date_lt: Time) {
                            viewer {
                                zones(filter: { zoneTag: $zone_tag }) {
                                    httpRequests1hGroups(
                                        filter: {
                                            datetime_gt: $date_gt,
                                            datetime_lt: $date_lt
                                        }, limit: 3 ) {
                                        sum {
                                            bytes
                                            cachedBytes
                                        }
                                    }
                                }
                            }
                        }
                    ",
                    Variables = new Dictionary<string, object>()
                    {
                        { "zone_tag", _configuration.GetValue<string>("Cloudflare:ZoneId") },
                        { "date_gt", yesterday.ToString("yyyy-MM-ddTHH:mm:ssZ").Replace(".", ":") },
                        { "date_lt", today.ToString("yyyy-MM-ddTHH:mm:ssZ").Replace(".", ":") }
                    }
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("Cloudflare:Token"));

                using (var response = await _graphQLClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        throw;
                    }

                    json = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return json;
        }

        [Authorize]
        [HttpGet]
        public async Task<string> GetBandwidthChart()
        {
            string json = string.Empty;

            try
            {
                if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) == UserRoleEnum.BASIC.ToString())
                {
                    HttpContext.Response.StatusCode = 403;
                    return json;
                }

                DateTime lastMonth = DateTime.Now.AddMonths(-1);

                GraphQLQuery query = new GraphQLQuery()
                {
                    Query = @"
                        query ($zone_tag: string, $date: Date) {
                            viewer {
                                zones(filter: { zoneTag: $zone_tag }) {
                                    httpRequests1dGroups(
                                        filter: {
                                            date_gt: $date
                                        }, 
                                        limit: 1000,
                                        orderBy: [date_ASC]) {
                                        dimensions {
                                            date
                                        }
                                        sum {
                                            bytes
                                            cachedBytes
                                        }
                                    }
                                }
                            }
                        }
                    ",
                    Variables = new Dictionary<string, object>()
                    {
                        { "zone_tag", _configuration.GetValue<string>("Cloudflare:ZoneId") },
                        { "date", lastMonth.ToString("yyyy-MM-dd") }
                    }
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration.GetValue<string>("Cloudflare:Token"));

                using (var response = await _graphQLClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        throw;
                    }

                    json = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return json;
        }
    }
}
