using Commons;
using Commons.Collections;
using Commons.Enums;
using GUI.Admin.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class WebshareController : Controller
    {
        private readonly ILogger<WebshareController> _logger;
        private readonly IConfiguration _configuration;

        private string _token;

        public WebshareController(ILogger<WebshareController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            AppSettings settings = new AppSettingsCollection().Get(0);
            _token = settings.Webshare.Token;
        }

        [Authorize]
        [HttpGet]
        public async Task<string> GetProxyStats()
        {
            string json = string.Empty;

            try
            {
                if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) == UserRoleEnum.BASIC.ToString())
                {
                    HttpContext.Response.StatusCode = 403;
                    return json;
                }

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _token);

                using(HttpClient client = new HttpClient() { BaseAddress = new Uri("https://proxy.webshare.io/api/proxy/stats/") })
                {
                    using (var response = await client.SendAsync(request))
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return json;
        }
    }
}
