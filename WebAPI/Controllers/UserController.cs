using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Attributes;
using WebAPI.Models;
using static Commons.AppSettings;

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
        private IConfiguration _configuration;
        private UserCollection _userCollection = new UserCollection();
        private readonly string _recaptchaBaseURL = "https://www.google.com/recaptcha/api/siteverify";
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public UserController(ILogger<UserController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves a specific User by user id
        /// </summary>
        /// <param name="id">The User id</param>
        [AllowAnonymous]
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
        [AllowAnonymous]
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
                        new List<User>());
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
        /// /// Create a new User
        /// </summary>
        /// <param name="g_recaptcha_response">The CAPTCHA status token</param>
        /// <param name="model">The User model</param>
        /// <returns></returns>
        [AllowAnonymous]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
        [HttpPut, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<APIResponse> Create(string g_recaptcha_response, [FromBody] User model)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                string recaptchaUrl = $"{_recaptchaBaseURL}?secret={_configuration.GetValue<string>("recaptcha_secret")}&response={g_recaptcha_response}&remoteip={HttpUtility.UrlEncode(Request.HttpContext.Connection.RemoteIpAddress.ToString())}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, recaptchaUrl);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                GRecaptchaResponse result = JsonConvert.DeserializeObject<GRecaptchaResponse>(await response.Content.ReadAsStringAsync());

                if (!result.Success)
                {
                    throw new APIException(System.Net.HttpStatusCode.Unauthorized,
                        "Invalid CAPTCHA",
                        "CAPTCHA result is not valid");
                }

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

                if(!new EmailAddressAttribute().IsValid(model.Email))
                {
                    throw new APIException(HttpStatusCode.UnprocessableEntity,
                        "Email not valid",
                        "Please provide a valid email");
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

                SmtpClient smtp = new SmtpClient(_configuration.GetValue<string>("smtp_host"), _configuration.GetValue<int>("smtp_port"))
                {
                    Credentials = new NetworkCredential(_configuration.GetValue<string>("smtp_username"), _configuration.GetValue<string>("smtp_password")),
                    EnableSsl = true
                };

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_configuration.GetValue<string>("smtp_address")),
                    Subject = "Welcome to AniAPI",
                    Body = $"Welcome <b>{model.Username}</b>,<br/>click on the link below to confirm your account and start using <a href=\"https://aniapi.com\" target=\"_blank\">AniAPI</a>.<br/><a href=\"https://api.aniapi.com/v1/auth/{model.Id}\" target=\"_blank\">Complete registration</a>",
                    IsBodyHtml = true
                };

                mail.To.Add(model.Email);

                smtp.Send(mail);

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
        [Attributes.Authorize]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
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

                if (!string.IsNullOrEmpty(model.Password))
                {
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
                        user.PasswordHash = argonConfig.EncodeString(hash.Buffer);
                    }

                    model.Password = null;
                }

                user.Gender = model.Gender;

                if(model.Localization != null)
                {
                    user.Localization = model.Localization;
                }

                if(model.AnilistId != null)
                {
                    user.AnilistId = model.AnilistId;

                    if (string.IsNullOrEmpty(model.AnilistToken))
                    {
                        throw new APIException(HttpStatusCode.BadRequest,
                            "Bad request",
                            "'anilist_id' field must come with 'anilist_token' field");
                    }
                    user.AnilistToken = model.AnilistToken;

                    if (string.IsNullOrEmpty(user.AvatarTracker))
                    {
                        user.AvatarTracker = "anilist";
                    }
                }

                if(model.MyAnimeListId != null)
                {
                    user.MyAnimeListId = model.MyAnimeListId;

                    if (string.IsNullOrEmpty(model.MyAnimeListToken))
                    {
                        throw new APIException(HttpStatusCode.BadRequest,
                            "Bad request",
                            "'mal_id' field must come with 'mal_token' field");
                    }
                    user.MyAnimeListToken = model.MyAnimeListToken;

                    if (string.IsNullOrEmpty(user.AvatarTracker))
                    {
                        user.AvatarTracker = "mal";
                    }
                }

                if(model.AvatarTracker != null)
                {
                    user.AvatarTracker = model.AvatarTracker;
                }

                this._userCollection.Edit(ref user);

                user.Email = null;
                user.EmailVerified = null;
                user.PasswordHash = null;
                user.LastLoginDate = null;
                user.Token = null;

                user.CalcDerivedFields();

                user.AnilistId = null;
                user.AnilistToken = null;
                user.MyAnimeListId = null;
                user.MyAnimeListToken = null;

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
        [Attributes.Authorize]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
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
