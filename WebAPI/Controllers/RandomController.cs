using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// R Controller for random resources
    /// </summary>
    [ApiVersion("1")]
    [Route("random")]
    [ApiController]
    public class RandomController : Controller
    {
        private readonly ILogger<RandomController> _logger;
        private AnimeCollection _animeCollection = new AnimeCollection();
        private AnimeSongCollection _animeSongCollection = new AnimeSongCollection();

        public RandomController(ILogger<RandomController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of random Anime
        /// </summary>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("anime"), MapToApiVersion("1")]
        [HttpGet("anime/{count}"), MapToApiVersion("1")]
        [HttpGet("anime/{count}/{nsfw}"), MapToApiVersion("1")]
        public APIResponse RandomAnime(int count = 1, bool nsfw = false, AnimeFormatEnum? format = null)
        {
            try
            {
                var query = this._animeCollection.Collection
                    .AsQueryable();

                if (!nsfw)
                {
                    query = query.Where(x => 
                        !x.Genres.Contains("Hentai") &&
                        !x.Genres.Contains("Nudity") &&
                        !x.Genres.Contains("Ecchi") &&
                        !x.Genres.Contains("Drugs") &&
                        !x.Genres.Contains("Rape") &&
                        !x.Genres.Contains("Handjob"));
                }

                if (format.HasValue)
                {
                    query = query.Where(x => x.Format == format.Value);
                }

                List<Anime> anime = query.Sample(count > 50 ? 50 : count).ToList();

                if (anime == null || anime.Count == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero anime found",
                        new List<Anime>());
                }

                return APIManager.SuccessResponse($"{anime.Count} random anime found", anime);
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
        /// Retrieves a list of random Song
        /// </summary>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("song"), MapToApiVersion("1")]
        [HttpGet("song/{count}"), MapToApiVersion("1")]
        public APIResponse RandomSong(int count = 1)
        {
            try
            {
                List<AnimeSong> animeSongs = this._animeSongCollection.Collection
                    .AsQueryable()
                    .Sample(count > 50 ? 50 : count)
                    .ToList();

                if (animeSongs == null || animeSongs.Count == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero songs found",
                        new List<AnimeSong>());
                }

                return APIManager.SuccessResponse($"{animeSongs.Count} random songs found", animeSongs);
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
