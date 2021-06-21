using Commons;
using Commons.Collections;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;

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

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate an User
        /// </summary>
        /// <param name="credentials">The User credentials</param>
        [EnableCors("CorsInternal")]
        [HttpPost, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse Login([FromBody] APICredentials credentials)
        {
            // TODO: mettere recaptcha

            try
            {
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
                user.AnilistToken = null;
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
    }
}
