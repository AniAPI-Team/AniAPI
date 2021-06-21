using Commons;
using Commons.Collections;
using Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// CRUD Controller for UserStory resources
    /// </summary>
    [ApiVersion("1")]
    [Route("user_story")]
    [ApiController]
    public class UserStoryController : Controller
    {
        private readonly ILogger<UserStoryController> _logger;
        private UserStoryCollection _userStoryCollection = new UserStoryCollection();
        private AnimeCollection _animeCollection = new AnimeCollection();
        private UserCollection _userCollection = new UserCollection();

        public UserStoryController(ILogger<UserStoryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a specific UserStory by id
        /// </summary>
        /// <param name="id">The UserStory id</param>
        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
        {
            try
            {
                UserStory story = this._userStoryCollection.Get(id);

                if (story == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Story not found",
                        $"Story with id {id} does not exists");
                }

                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC && authenticatedUser.Id != story.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to get this story");
                }

                return APIManager.SuccessResponse("Story found", story);
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
        /// Retrieves a list of UserStory
        /// </summary>
        /// <param name="filter">The UserStory filter</param>
        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] UserStoryFilter filter)
        {
            try
            {
                Paging<UserStory> result = this._userStoryCollection.GetList<UserStoryFilter>(filter);

                if (result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero stories found",
                        "");
                }

                if (filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                User authenticatedUser = (User)HttpContext.Items["user"];

                foreach (UserStory story in result.Documents)
                {
                    if (authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC && authenticatedUser.Id != story.UserID)
                    {
                        throw new APIException(HttpStatusCode.Forbidden,
                            "Forbidden",
                            "You have no access rights to get a story found");
                    }
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} stories. Last page number is {result.LastPage} for a total of {result.Count} stories", result);
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
        /// Create a new UserStory
        /// </summary>
        /// <param name="model">The UserStory model</param>
        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpPut, MapToApiVersion("1")]
        public APIResponse Create([FromBody] UserStory model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if(authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC && authenticatedUser.Id != model.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to add this story");
                }

                if (this._userCollection.Get(model.UserID) == null)
                {
                    throw new APIException(HttpStatusCode.BadRequest,
                        "User not found",
                        "Please provide a valid user_id");
                }

                if (this._animeCollection.Get(model.AnimeID) == null)
                {
                    throw new APIException(HttpStatusCode.BadRequest,
                        "Anime not found",
                        "Please provide a valid anime_id");
                }

                List<UserStory> collisions = new List<UserStory>();

                collisions.AddRange(this._userStoryCollection.GetList(new UserStoryFilter()
                {
                    user_id = model.UserID,
                    anime_id = model.AnimeID
                }).Documents);

                if (collisions.Count > 0)
                {
                    throw new APIException(HttpStatusCode.Conflict,
                        "Duplicated story",
                        "Already exists a story with these user_id and anime_id");
                }

                collisions = null;

                model.Synced = false;

                this._userStoryCollection.Add(ref model);

                return APIManager.SuccessResponse("Story created", model);
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
        /// Update an existing UserStory
        /// </summary>
        /// <param name="model">The UserStory model</param>
        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] UserStory model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC && authenticatedUser.Id != model.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to edit this story");
                }

                if (!this._userStoryCollection.Exists(ref model, false))
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Story not found",
                        $"Story with id {model.Id} does not exists");
                }

                UserStory story = this._userStoryCollection.Get(model.Id);

                story.Status = model.Status;
                story.CurrentEpisode = model.CurrentEpisode;
                story.CurrentEpisodeTime = model.CurrentEpisodeTime;
                story.Synced = false;

                this._userStoryCollection.Edit(ref story);

                return APIManager.SuccessResponse("Story updated", story);
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
        /// Delete an existing UserStory by id
        /// </summary>
        /// <param name="id">The UserStory id</param>
        [Authorize]
        [EnableCors("CorsInternal")]
        [HttpDelete("{id}"), MapToApiVersion("1")]
        public APIResponse Delete(long id)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                UserStory story = this._userStoryCollection.Get(id);

                if (story == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Story not found",
                        $"Story with id {id} does not exists");
                }

                if (authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC && authenticatedUser.Id != story.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to delete this story");
                }

                this._userStoryCollection.Delete(id);

                return APIManager.SuccessResponse("Story deleted");
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
