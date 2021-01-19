using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.Collections;
using Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Models;

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
        public Anime GetOne(long id)
        {
            try
            {
                Anime anime = this._animeCollection.Get(id);
                if(anime == null)
                {
                    throw new ApiException(HttpStatusCode.NotFound,
                        "Anime not found :(",
                        $"Anime with id {id} does not exists");
                }

                return anime;
            }
            catch (ApiException ex)
            {
                this._logger.LogError(ex.Message);
                //return ApiResponse.Error(ex);
                return null;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                //return ApiResponse.Error(ex);
                return null;
            }
        }

        [HttpGet, MapToApiVersion("1")]
        public JsonResult GetMore([FromQuery] AnimeFilter filter)
        {
            return new JsonResult(null);
        }

        [HttpPut, MapToApiVersion("1")]
        public JsonResult Create([FromBody] Anime model)
        {
            return new JsonResult(null);
        }

        [HttpPost, MapToApiVersion("1")]
        public JsonResult Update([FromBody] Anime model)
        {
            return new JsonResult(null);
        }

        [HttpDelete("{id}"), MapToApiVersion("1")]
        public JsonResult Delete(long id)
        {
            return new JsonResult(null);
        }
    }
}
