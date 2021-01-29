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

        [HttpGet("MockUpResponse"), MapToApiVersion("1")]
        public APIResponse TestAnimeApi()
        {
            Anime anime = new Anime();
            anime.Format = AnimeFormatEnum.TV;
            anime.EpisodesCount = 35;
            anime.EpisodeDuration = 23;
            anime.Status = AnimeStatusEnum.FINISHED;
            anime.StartDate = DateTime.Now;
            anime.SeasonPeriod = AnimeSeasonEnum.SUMMER;
            anime.SeasonYear = 2018;
            anime.Score = 79;
            anime.Genres = new List<string>() { "Action", "Drama", "Fantasy", "Mystery", "che ne so!!" };
            anime.CoverImage = "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/bx104578-LaZYFkmhinfB.jpg";
            anime.BannerImage = "https://s4.anilist.co/file/anilistcdn/media/anime/banner/104578-z7SadpYEuAsy.jpg";

            anime.Titles = new Dictionary<string, string>();
            anime.Titles.Add(LocalizationEnum.English, "Shingeki no Kyojin 3 Part 2");

            anime.Descriptions = new Dictionary<string, string>();
            anime.Descriptions.Add(LocalizationEnum.English, "The second cour of <i>Shingeki no Kyojin 3</i>.<br><br>The battle to retake Wall Maria begins now! With Eren’s new hardening ability, the Scouts are confident they can seal the wall and take back Shiganshina District. If they succeed, Eren can finally unlock the secrets of the basement—and the world. But danger lies in wait as Reiner, Bertholdt, and the Beast Titan have plans of their own. Could this be humanity’s final battle for survival?<br><br>(Source: Funimation)");

            return APIManager.SuccessResponse("Have fun with testing", anime);
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
