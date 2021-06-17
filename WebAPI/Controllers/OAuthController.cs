using Commons;
using Commons.Collections;
using Commons.Filters;
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
    /// Controller for oauth requests
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

        //https://localhost:44349/api/v1/oauth?client_id=fd8482df-6554-4efe-be14-8354a0e2d3d0&response_type=code&redirect_uri=http%3A%2F%2Flocalhost
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

        [HttpPost, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse Authenticate([FromBody] APICredentials credentials)
        {
            OAuthRequest request = JsonConvert.DeserializeObject<OAuthRequest>(HttpContext.Session.GetString("oauth"));

            APIResponse login = _authController.Login(credentials);

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

                        _cache.GetOrCreate()
                        _generatedCodes.Add(Request.HttpContext.Connection.RemoteIpAddress.ToString(), 
                            new Tuple<string, string>(code, user.Token));

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

        [Route("token")]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Token(string client_id, string client_secret, string code, string redirect_uri)
        {
            // TODO: verificare code con quello generato lato server
            // se è corretto, ritornare token dell'utente

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

            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            
            if(!_generatedCodes.ContainsKey(ip))
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.Forbidden,
                            "Forbidden",
                            "code not valid"));
            }
            
            if(code != _generatedCodes[ip].Item1)
            {
                return APIManager.ErrorResponse(new APIException(
                            System.Net.HttpStatusCode.Forbidden,
                            "Forbidden",
                            "code not valid"));
            }

            return APIManager.SuccessResponse("Code verified", _generatedCodes[ip].Item2);
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
