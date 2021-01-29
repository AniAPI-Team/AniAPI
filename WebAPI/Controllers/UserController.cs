using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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
            return APIManager.SuccessResponse();
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
