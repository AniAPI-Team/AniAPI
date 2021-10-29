using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class AnimeController : Controller
    {
        private readonly ILogger<AnimeController> _logger;

        private AnimeSuggestionCollection _animeSuggestionCollection;

        public AnimeController(ILogger<AnimeController> logger)
        {
            _logger = logger;

            _animeSuggestionCollection = new AnimeSuggestionCollection();
        }

        [Authorize]
        [HttpGet]
        [Route("Anime/GetSuggestionsByWebsite/{animeId}/{websiteName}")]
        public List<AnimeSuggestion> GetSuggestionsByWebsite(long animeId, string websiteName)
        {
            try
            {
                var builder = Builders<AnimeSuggestion>.Filter;
                var filter = builder.Eq("anime_id", animeId) & builder.Eq("source", websiteName);

                return _animeSuggestionCollection.Collection.Find(filter).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }
    }
}
