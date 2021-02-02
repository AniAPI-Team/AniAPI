using Commons;
using Commons.Collections;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Controllers
{
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

        [HttpPost, MapToApiVersion("1")]
        public APIResponse Login([FromBody] APICredentials credentials)
        {
            try
            {
                var builder = Builders<User>.Filter;
                FilterDefinition<User> filter = builder.Eq("email", credentials.Email);

                User user = new User()
                {
                    Email = credentials.Email,
                    Password = credentials.Password
                };

                if(!this._userCollection.Exists(ref user, true))
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
                    Subject = new ClaimsIdentity(new [] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                
                user.Token = tokenHandler.WriteToken(token);
                user.PasswordHash = null;
                
                return APIManager.SuccessResponse("Login done", user);
            }
            catch(APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch(Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }
    }
}
