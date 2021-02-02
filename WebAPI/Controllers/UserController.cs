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
using WebAPI.Attributes;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /// <summary>
    /// CRUD Controller for User model
    /// </summary>
    [ApiVersion("1")]
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private UserCollection _userCollection = new UserCollection();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a specific user by user id
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

                user.HideConfindentialValues();

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

        /// <summary>
        /// Retrieve a list of user
        /// </summary>
        /// <param name="filter">The MongoDB query filter</param>
        /// <returns></returns>
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] UserFilter filter)
        {
            return APIManager.SuccessResponse();
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model">The user model</param>
        /// <returns></returns>
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
                    username = model.Username
                }).Documents);

                if(collisions.Count > 0)
                {
                    throw new APIException(HttpStatusCode.Conflict,
                        "Duplicated username",
                        "The username provided is already used by another user");
                }

                collisions.AddRange(this._userCollection.GetList(new UserFilter()
                {
                    email = model.Email
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

                model.Password = null;
                model.Role = UserRoleEnum.BASIC;
                model.EmailVerified = false;

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

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="model">The user model</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] User model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if(authenticatedUser.Role == UserRoleEnum.BASIC && authenticatedUser.Id != model.Id)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to edit this user");
                }

                if(!this._userCollection.Exists(ref model, false))
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "User not found",
                        $"User with id {model.Id} does not exists");
                }

                User user = this._userCollection.Get(model.Id);
                user.Localization = model.Localization;

                this._userCollection.Edit(ref user);

                return APIManager.SuccessResponse("User updated", user);
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
        /// Delete an existing user by user id
        /// </summary>
        /// <param name="id">The user id</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("{id}"), MapToApiVersion("1")]
        public APIResponse Delete(long id)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser.Role == UserRoleEnum.BASIC && authenticatedUser.Id != id)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to edit this user");
                }

                User user = new User()
                {
                    Id = id
                };

                if (!this._userCollection.Exists(ref user, false))
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "User not found",
                        $"User with id {user.Id} does not exists");
                }

                this._userCollection.Delete(user.Id);

                return APIManager.SuccessResponse("User deleted");
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
