using Commons;
using Commons.Collections;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for authentication requests
    /// </summary>
    [ApiVersion("1")]
    [Route("auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private UserCollection _userCollection = new UserCollection();
        private readonly string _recaptchaBaseURL = "https://www.google.com/recaptcha/api/siteverify";

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate an User
        /// </summary>
        /// <param name="credentials">The User credentials</param>
        [AllowAnonymous]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
        [HttpPost, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<APIResponse> Login([FromBody] APICredentials credentials)
        {
            try
            {
#if !DEBUG
                HttpClient httpClient = new HttpClient();

                string recaptchaUrl = $"{_recaptchaBaseURL}?secret={_configuration.GetValue<string>("recaptcha_secret")}&response={credentials.GRecaptchaResponse}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, recaptchaUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                GRecaptchaResponse result = JsonConvert.DeserializeObject<GRecaptchaResponse>(await response.Content.ReadAsStringAsync());

                if (!result.Success)
                {
                    throw new APIException(System.Net.HttpStatusCode.Unauthorized,
                        "Invalid CAPTCHA",
                        "CAPTCHA result is not valid");
                }
#endif

                var builder = Builders<User>.Filter;
                FilterDefinition<User> filter = builder.Eq("email", credentials.Email);

                User user = new User()
                {
                    Email = credentials.Email,
                    Password = credentials.Password
                };

                if (!this._userCollection.Exists(ref user, true))
                {
                    throw new APIException(System.Net.HttpStatusCode.Unauthorized,
                        "Invalid login",
                        "Credentials are not valid");
                }
                else
                {
                    user = this._userCollection.Get(user.Id);
                }

                bool passwordOk = Argon2.Verify(user.PasswordHash, credentials.Password);

                if (!passwordOk)
                {
                    throw new APIException(System.Net.HttpStatusCode.Unauthorized,
                        "Invalid login",
                        "Credentials are not valid");
                }

                user.Password = null;
                user.LastLoginDate = DateTime.Now;
                this._userCollection.Edit(ref user);

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes((string)_configuration.GetValue(typeof(string), "jwt_secret"));

                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                user.Token = tokenHandler.WriteToken(token);
                user.PasswordHash = null;

                user.CalcDerivedFields();

                user.AnilistId = null;
                user.AnilistToken = null;
                user.MyAnimeListId = null;
                user.MyAnimeListToken = null;

                return APIManager.SuccessResponse("Login done", user);
            }
            catch (APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }

        /// <summary>
        /// Verify an User email address
        /// </summary>
        /// <param name="id">The User id</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("{id}"), MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse VerifyEmail(long id)
        {
            try
            {
                User user = this._userCollection.Get(id);

                if (user == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "User not found",
                        $"User with id {id} does not exists");
                }

                user.EmailVerified = true;

                this._userCollection.Edit(ref user);

                return APIManager.SuccessResponse("Email verified");
            }
            catch (APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }

        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpGet("me"), MapToApiVersion("1")]
        public APIResponse Me()
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "User not found",
                        $"The token you provided has no user on it");
                }

                authenticatedUser.PasswordHash = null;
                authenticatedUser.LastLoginDate = null;
                authenticatedUser.Token = null;

                authenticatedUser.CalcDerivedFields();

                authenticatedUser.AnilistId = null;
                authenticatedUser.AnilistToken = null;
                authenticatedUser.MyAnimeListId = null;
                authenticatedUser.MyAnimeListToken = null;

                return APIManager.SuccessResponse($"Hi {authenticatedUser.Username}", authenticatedUser);
            }
            catch (APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }
    }
}
