using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        
        private AnimeCollection _animeCollection;
        private AnimeSuggestionCollection _animeSuggestionCollection;
        private EpisodeCollection _episodeCollection;
        private AnimeSongCollection _animeSongCollection;
        private UserCollection _userCollection;
        private ServicesStatusCollection _servicesStatusCollection;
        private WebsiteCollection _websiteCollection;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;

            _animeCollection = new AnimeCollection();
            _animeSuggestionCollection = new AnimeSuggestionCollection();
            _episodeCollection = new EpisodeCollection();
            _animeSongCollection = new AnimeSongCollection();
            _userCollection = new UserCollection();
            _servicesStatusCollection = new ServicesStatusCollection();
            _websiteCollection = new WebsiteCollection();
        }

        [Authorize]
        [HttpGet]
        public Dictionary<string, int[]> GetResourcesMetrics()
        {
            Dictionary<string, int[]> metrics = new Dictionary<string, int[]>();

            try
            {
                DateTime lastMonth = DateTime.Now.AddMonths(-1);

                metrics["anime"] = new int[2]
                {
                    (int)_animeCollection.Collection.CountDocuments(Builders<Anime>.Filter.Empty),
                    (int)_animeCollection.Collection.CountDocuments(Builders<Anime>.Filter.Gt("creation_date", lastMonth))
                };

                metrics["episode"] = new int[2]
                {
                    (int)_episodeCollection.Collection.CountDocuments(Builders<Episode>.Filter.Empty),
                    (int)_episodeCollection.Collection.CountDocuments(Builders<Episode>.Filter.Gt("creation_date", lastMonth))
                };

                metrics["song"] = new int[2]
                {
                    (int)_animeSongCollection.Collection.CountDocuments(Builders<AnimeSong>.Filter.Empty),
                    (int)_animeSongCollection.Collection.CountDocuments(Builders<AnimeSong>.Filter.Gt("creation_date", lastMonth))
                };

                metrics["user"] = new int[2]
                {
                    (int)_userCollection.Collection.CountDocuments(Builders<User>.Filter.Empty),
                    (int)_userCollection.Collection.CountDocuments(Builders<User>.Filter.Gt("creation_date", lastMonth))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return metrics;
        }

        [Authorize]
        [HttpGet]
        public Dictionary<string, double> GetServicesStatus()
        {
            Dictionary<string, double> status = new Dictionary<string, double>();

            try
            {
                List<ServicesStatus> services = _servicesStatusCollection.Collection.Find(Builders<ServicesStatus>.Filter.Empty).ToList();

                foreach(ServicesStatus s in services)
                {
                    status.Add(s.Name, s.Progress);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return status;
        }

        [Authorize]
        [HttpGet]
        public Dictionary<string, int[]> GetAnimeMatchedCount()
        {
            Dictionary<string, int[]> count = new Dictionary<string, int[]>();

            try
            {
                int animeCount = (int)_animeCollection.Collection.CountDocuments(Builders<Anime>.Filter.Empty);

                foreach (Website w in _websiteCollection.Collection.Find(Builders<Website>.Filter.Empty).ToList())
                {
                    var matched = _episodeCollection.Collection.Aggregate()
                        .Match(Builders<Episode>.Filter.Eq("source", w.Name))
                        .Group(new BsonDocument("_id", new BsonDocument
                        {
                            { "anime_id", "$anime_id" },
                            { "source", "$source" }
                        }));

                    var possible = _animeSuggestionCollection.Collection.Aggregate()
                        .Match(Builders<AnimeSuggestion>.Filter.Eq("source", w.Name))
                        .Group(new BsonDocument("_id", new BsonDocument
                        {
                            { "anime_id", "$anime_id" },
                            { "source", "$source" }
                        }));

                    count[w.Name] = new int[3];

                    count[w.Name][0] = (matched.ToList().Count * 100) / animeCount;
                    count[w.Name][1] = (possible.ToList().Count * 100) / animeCount;
                    count[w.Name][2] = 100 - count[w.Name][0] - count[w.Name][1];
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return count;
        }
    }
}
