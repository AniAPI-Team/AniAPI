using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiVersion("1")]
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private UserCollection _userCollection = new UserCollection();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific user by user id
        /// </summary>
        /// <param name="id">The user id</param>
        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
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

                return APIManager.SuccessResponse("User found", user);
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

        [HttpGet("MockUpResponse"), MapToApiVersion("1")]
        public APIResponse TestUserApi()
        {
            User user = new User()
            {
                Id = 1,
                Username = "Pippo",
                Email = "pippo@gmail.com",
                Localization = LocalizationEnum.Italian,
                Gender = UserGenderEnum.MALE,
            };

            return APIManager.SuccessResponse("Have fun with testing", user);
        }

        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] UserFilter filter)
        {
            return APIManager.SuccessResponse();
        }

        [HttpPut, MapToApiVersion("1")]
        public APIResponse Create([FromBody] User model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username))
                {
                    throw new APIException(HttpStatusCode.Unauthorized,
                        "Missing username",
                        "Please provide a valid username");
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    throw new APIException(HttpStatusCode.Unauthorized,
                        "Missing email",
                        "Please provide a valid email");
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    throw new APIException(HttpStatusCode.Unauthorized,
                        "Missing password",
                        "Please provide a valid password");
                }

                List<User> collisions = new List<User>();

                collisions.AddRange(this._userCollection.GetList(new UserFilter()
                {
                    Username = model.Username
                }).Documents);

                if(collisions.Count > 0)
                {
                    throw new APIException(HttpStatusCode.Conflict,
                        "Duplicated username",
                        "The username provided is already used by another user");
                }

                collisions.AddRange(this._userCollection.GetList(new UserFilter()
                {
                    Email = model.Email
                }).Documents);

                if (collisions.Count > 0)
                {
                    throw new APIException(HttpStatusCode.Conflict,
                        "Duplicated email",
                        "The email provided is already used by another user");
                }

                collisions = null;

                byte[] passwordBytes = Encoding.UTF8.GetBytes(model.Password);
                byte[] salt = new byte[16];
                _rng.GetBytes(salt);

                Argon2Config argonConfig = new Argon2Config()
                {
                    Type = Argon2Type.DataIndependentAddressing,
                    Version = Argon2Version.Nineteen,
                    TimeCost = 10,
                    MemoryCost = 32768,
                    Lanes = 5,
                    Threads = Environment.ProcessorCount,
                    Password = passwordBytes,
                    Salt = salt,
                    HashLength = 20
                };
                Argon2 argon = new Argon2(argonConfig);

                using (SecureArray<byte> hash = argon.Hash())
                {
                    model.PasswordHash = argonConfig.EncodeString(hash.Buffer);
                }

                model.Password = string.Empty;
                model.Role = UserRoleEnum.BASIC;

                this._userCollection.Add(ref model);

                return APIManager.SuccessResponse("User created", model);
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

        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] User model)
        {
            return APIManager.SuccessResponse();
        }

        [HttpDelete("{id}"), MapToApiVersion("1")]
        public APIResponse Delete(long id)
        {
            return APIManager.SuccessResponse();
        }
    }
}
