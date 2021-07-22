using Commons;
using Commons.Collections;
using Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for OAuth requests
    /// </summary>
    [ApiVersion("1")]
    [Route("oauth")]
    [ApiController]
    public class OAuthController : Controller
    {
        private readonly ILogger<OAuthController> _logger;
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;
        private OAuthClientCollection _oAuthClientCollection = new OAuthClientCollection();
        private AuthController _authController;

        public OAuthController(ILogger<OAuthController> logger, IConfiguration configuration, IMemoryCache cache)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;

            _authController = new AuthController(null, configuration);
        }

        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index(string client_id, string redirect_uri, string response_type, string state)
        {
            if(string.IsNullOrEmpty(client_id) ||
                string.IsNullOrEmpty(redirect_uri) ||
                string.IsNullOrEmpty(response_type))
            {
                return BadRequest("missing required field");
            }

            if(response_type != "token" && response_type != "code")
            {
                return BadRequest("response_type content not expected");
            }

            OAuthRequest request = new OAuthRequest()
            {
                ClientID = client_id,
                RedirectURI = redirect_uri,
                ResponseType = response_type,
                State = state
            };

            HttpContext.Session.SetString("oauth", JsonConvert.SerializeObject(request));

            List<OAuthClient> clients = _oAuthClientCollection.GetList(new OAuthClientFilter()
            {
                client_id = Guid.Parse(request.ClientID)
            }).Documents;

            if(clients.Count == 0)
            {
                return NotFound();
            }

            OAuthClient client = clients[0];

            if(redirect_uri != client.RedirectURI)
            {
                return Forbid("redirect_uri content mismatch from registered client");
            }

            return View(client);
        }

        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpPost, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<APIResponse> Authenticate([FromBody] APICredentials credentials)
        {
            OAuthRequest request = JsonConvert.DeserializeObject<OAuthRequest>(HttpContext.Session.GetString("oauth"));

            APIResponse login = await _authController.Login(credentials);

            if(login.StatusCode == System.Net.HttpStatusCode.OK)
            {
                User user = (User)login.Data;
                string query = string.Empty;

                switch (request.ResponseType)
                {
                    case "token":
                        query = $"#access_token={user.Token}";
                        break;
                    case "code":
                        string code = generateCode();

                        _cache.Set(code,
                            user.Token,
                            new MemoryCacheEntryOptions()
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                            });

                        query = $"?code={code}";
                        break;
                    default:
                        return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.BadRequest,
                            "Bad request",
                            "response_type not expected"));
                }

                if (!string.IsNullOrEmpty(request.State))
                {
                    query += $"&state={request.State}";
                }

                Uri uri = new Uri(new Uri(request.RedirectURI), query);

                return APIManager.SuccessResponse("Authentication done", uri);
            }

            return login;
        }

        /// <summary>
        /// Exchange an OAuth code for an User token
        /// </summary>
        /// <param name="client_id">The OAauthClient client_id</param>
        /// <param name="client_secret">The OAuthClient client_secret</param>
        /// <param name="code">The generated code</param>
        /// <param name="redirect_uri">The OAuthClient redirect_uri</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("token")]
        [EnableCors("CorsEveryone")]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Token(string client_id, string client_secret, string code, string redirect_uri)
        {
            if (string.IsNullOrEmpty(client_id) ||
                string.IsNullOrEmpty(redirect_uri) ||
                string.IsNullOrEmpty(client_secret) ||
                string.IsNullOrEmpty(code))
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.BadRequest,
                            "Bad request",
                            "Missing required field"));
            }

            List<OAuthClient> clients = _oAuthClientCollection.GetList(new OAuthClientFilter()
            {
                client_id = Guid.Parse(client_id)
            }).Documents;

            if (clients.Count == 0)
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.NotFound,
                            "Not found",
                            "client_id not found"));
            }

            OAuthClient client = clients[0];

            if(Guid.Parse(client_secret) != client.ClientSecret)
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.Forbidden,
                            "Forbidden",
                            "client_secret content mismatch from registered client"));
            }

            if (redirect_uri != client.RedirectURI)
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.Forbidden,
                            "Forbidden",
                            "redirect_uri content mismatch from registered client"));
            }

            if(!_cache.TryGetValue(code, out string token))
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.Forbidden,
                            "Forbidden",
                            "code not valid"));
            }
            
            return APIManager.SuccessResponse("Code verified", token);
        }

        private Random _random = new Random();
        private string generateCode()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 30)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
