using Commons;
using Commons.Collections;
using GUI.Admin.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;
        private IUserManager _userManager;

        private Random _random;
        private AnimeCollection _animeCollection;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IMemoryCache cache, IUserManager userManager)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            _userManager = userManager;

            _animeCollection = new AnimeCollection();

            _random = new Random((int)DateTime.Now.Ticks);
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Page");
            }

            string state = getClientState();

            _cache.Set(Request.HttpContext.Connection.RemoteIpAddress,
                state,
                new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                });

            ViewBag.ClientId = _configuration.GetValue<string>("AniAPI:ClientId");
            ViewBag.RedirectUri = _configuration.GetValue<string>("AniAPI:RedirectUri");
            ViewBag.State = state;

            return View();
        }

        [HttpGet("/random_anime_pictures/{count}")]
        public List<string[]> GetRandomAnimePictures(int count)
        {
            List<string[]> pictures = new List<string[]>();

            try
            {
                long max = this._animeCollection.Last().Id;

                for (int i = 0; i < count; i++)
                {
                    Anime anime = null;
                    do
                    {
                        long id = _random.Next(1, (int)max) + 1;
                        anime = this._animeCollection.Get(id);
                    }
                    while (anime.NSFW == true || !anime.HasCoverImage);

                    pictures.Add(new string[2] { anime.CoverColor, anime.CoverImage });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return pictures;
        }
        
        public async Task<IActionResult> Authenticate(string code, string state = null)
        {
            try
            {
                if (!_cache.TryGetValue(Request.HttpContext.Connection.RemoteIpAddress, out string myState))
                {
                    return Forbid();
                }

                if (state != myState)
                {
                    return Forbid();
                }

                User user = null;

                using (HttpClient client = new HttpClient())
                {
                    string token = null;
                    string url = $"https://api.aniapi.com/v1/oauth/token?client_id={_configuration.GetValue<string>("AniAPI:ClientId")}&client_secret={_configuration.GetValue<string>("AniAPI:ClientSecret")}&code={code}&redirect_uri={_configuration.GetValue<string>("AniAPI:RedirectUri")}";

                    using (var response = await client.PostAsync(url, null))
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch
                        {
                            return Forbid();
                        }

                        token = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync())["data"];
                    }

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get
                    };
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    request.RequestUri = new Uri("https://api.aniapi.com/v1/auth/me");

                    using (var response = await client.SendAsync(request))
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch
                        {
                            return Forbid();
                        }

                        user = JsonConvert.DeserializeObject<User>(JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync()).Data.ToString());
                        user.Token = token;
                    }
                }
                
                await _userManager.SignIn(HttpContext, user, false);

                return RedirectToAction("Dashboard", "Page");
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userManager.SignOut(HttpContext);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        string getClientState()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 16)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
