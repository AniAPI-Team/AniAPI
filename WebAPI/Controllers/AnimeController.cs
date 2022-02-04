using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using System;
using System.Collections.Generic;
using System.Net;
using WebAPI.Models;
using MongoService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    /// <summary>
    /// R Controller for Anime resource
    /// </summary>
    [ApiVersion("1")]
    [Route("anime")]
    [ApiController]
    public class AnimeController : Controller
    {
        private readonly ILogger<AnimeController> _logger;
        private AnimeCollection _animeCollection = new AnimeCollection();

        public AnimeController(ILogger<AnimeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific Anime by id
        /// </summary>
        /// <param name="id">The Anime id</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
        {
            try
            {
                Anime anime = this._animeCollection.Get(id);

                if (anime == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Anime not found",
                        $"Anime with id {id} does not exists");
                }

                return APIManager.SuccessResponse("Anime found", anime);
            }
            catch (APIException ex)
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
        /// Retrieves a list of Anime
        /// </summary>
        /// <param name="filter">The Anime filter</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] AnimeFilter filter)
        {
            try
            {
                Paging<Anime> result = this._animeCollection.GetList<AnimeFilter>(filter);

                if(result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero anime found",
                        new List<Anime>());
                }

                if(filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} anime. Last page number is {result.LastPage} for a total of {result.Count} anime", result);
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

        [Authorize]
        [EnableCors("CorsEveryone")]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] Anime model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser.Role == Commons.Enums.UserRoleEnum.BASIC)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to edit this anime");
                }

                if (!this._animeCollection.Exists(ref model, false))
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Not found",
                        $"Anime with id {model.Id} does not exists");
                }

                Anime anime = this._animeCollection.Get(model.Id);

                // TODO: Logic to update anime document

                this._animeCollection.Edit(ref anime);

                return APIManager.SuccessResponse("Anime updated", anime);
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
