using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoService;
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
    /// CRUD Controller for User resource
    /// </summary>
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
        /// Retrieves a specific User by user id
        /// </summary>
        /// <param name="id">The User id</param>
        [EnableCors("CorsEveryone")]
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

        /// <summary>
        /// Retrieves a list of User
        /// </summary>
        /// <param name="filter">The User filter</param>
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] UserFilter filter)
        {
            try
            {
                Paging<User> result = this._userCollection.GetList<UserFilter>(filter);

                if (result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero users found",
                        "");
                }

                if (filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                foreach(User user in result.Documents)
                {
                    user.HideConfindentialValues();
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} users. Last page number is {result.LastPage} for a total of {result.Count} users", result);
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
        /// Create a new User
        /// </summary>
        /// <param name="model">The User model</param>
        [EnableCors("CorsInternal")]
        [HttpPut, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse Create([FromBody] User model)
        {
            // TODO: mettere recaptcha
            try
            {
                if (string.IsNullOrEmpty(model.Username))
                {
                    throw new APIException(HttpStatusCode.BadRequest,
                        "Missing username",
                        "Please provide a valid username");
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    throw new APIException(HttpStatusCode.BadRequest,
                        "Missing email",
                        "Please provide a valid email");
                }

                if (string.IsNullOrEmpty(model.Password))
                {
                    throw new APIException(HttpStatusCode.BadRequest,
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
        /// Update an existing User
        /// </summary>
        /// <param name="model">The User model</param>
        [Authorize]
        [EnableCors("CorsInternal")]
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
        /// Delete an existing User by id
        /// </summary>
        /// <param name="id">The User id</param>
        [Authorize]
        [EnableCors("CorsInternal")]
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
