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

namespace WebAPI.Controllers
{
    [ApiVersion("1")]
    [Route("episode")]
    [ApiController]
    public class EpisodeController : Controller
    {
        private readonly ILogger<EpisodeController> _logger;
        private EpisodeCollection _episodeCollection = new EpisodeCollection();

        public EpisodeController(ILogger<EpisodeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific episode by episode id
        /// </summary>
        /// <param name="id">The anime id</param>
        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
        {
            try
            {
                Episode episode = this._episodeCollection.Get(id);
                if (episode == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Epsiode not found",
                        $"Episode with id {id} does not exists");
                }

                return APIManager.SuccessResponse("Episode found", episode);
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
        /// Retrieves a list of episode
        /// </summary>
        /// <param name="filter">The episode filter</param>
        /// <returns></returns>
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] EpisodeFilter filter)
        {
            try
            {
                Paging<Episode> result = this._episodeCollection.GetList<EpisodeFilter>(filter);

                if(result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero episode found",
                        "");
                }

                if(filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} episodes. Last page number is {result.LastPage} for a total of {result.Count} episodes", result);
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
