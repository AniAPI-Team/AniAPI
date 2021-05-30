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
        /// Retrieves a specific anime by anime id
        /// </summary>
        /// <param name="id">The anime id</param>
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
                        "");
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

        [HttpPut, MapToApiVersion("1")]
        public APIResponse Create([FromBody] Anime model)
        {
            return APIManager.SuccessResponse();
        }

        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] Anime model)
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
