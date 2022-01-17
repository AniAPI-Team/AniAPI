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
    /// R Controller for AnimeSong resource
    /// </summary>
    [ApiVersion("1")]
    [Route("song")]
    [ApiController]
    public class AnimeSongController : Controller
    {
        private readonly ILogger<AnimeSongController> _logger;
        private AnimeSongCollection _animeSongCollection = new AnimeSongCollection();

        public AnimeSongController(ILogger<AnimeSongController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific AnimeSong by id
        /// </summary>
        /// <param name="id">The AnimeSong id</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
        {
            try
            {
                AnimeSong song = this._animeSongCollection.Get(id);

                if (song == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Song not found",
                        $"Song with id {id} does not exists");
                }

                return APIManager.SuccessResponse("Song found", song);
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
        /// Retrieves a list of AnimeSong
        /// </summary>
        /// <param name="filter">The AnimeSong filter</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] AnimeSongFilter filter)
        {
            try
            {
                Paging<AnimeSong> result = this._animeSongCollection.GetList<AnimeSongFilter>(filter);

                if (result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero songs found",
                        new List<AnimeSong>());
                }

                if (filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} songs. Last page number is {result.LastPage} for a total of {result.Count} songs", result);
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
