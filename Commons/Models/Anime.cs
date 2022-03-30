using Commons.Collections;
using Commons.Filters;
using MongoService;
using System;
using System.Collections.Generic;
using Commons.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Linq;

namespace Commons
{
    public class Anime : IModel
    {
        private AnimeCollection _animeCollection = new AnimeCollection();

        public Anime() { }

        public Anime(AnilistResponse.ResponseMedia media)
        {
            this.Titles = new Dictionary<string, string>();
            this.Titles[LocalizationEnum.Romaji] = media.Title.Romaji;
            this.Titles[LocalizationEnum.English] = media.Title.English;
            this.Titles[LocalizationEnum.Japanese] = media.Title.Native;

            this.Descriptions = new Dictionary<string, string>();
            this.Descriptions[LocalizationEnum.English] = media.Description;

            this.AnilistId = media.Id;
            this.MyAnimeListId = media.IdMal;
            this.Format = media.Format;
            this.Status = media.Status.HasValue ? media.Status.Value : AnimeStatusEnum.FINISHED;

            AnilistResponse.MediaDate correctDate = CheckDate(media.StartDate);
            this.StartDate = new DateTime(correctDate.Year.Value, correctDate.Month.Value, correctDate.Day.Value);

            correctDate = CheckDate(media.EndDate);
            this.EndDate = new DateTime(correctDate.Year.Value, correctDate.Month.Value, correctDate.Day.Value);

            this.SeasonPeriod = media.Season.HasValue ? media.Season.Value : AnimeSeasonEnum.UNKNOWN;
            this.SeasonYear = media.SeasonYear;

            if (media.Episodes.HasValue)
            {
                this.EpisodesCount = media.Episodes.Value;
            }
            else if (media.Status == AnimeStatusEnum.RELEASING && media.AiringEpisode != null)
            {
                this.EpisodesCount = media.AiringEpisode.Episode - 1;
            }
            else
            {
                this.EpisodesCount = 0;
            }

            this.EpisodeDuration = media.Duration;

            if (media.Trailer != null)
            {
                switch (media.Trailer.Site)
                {
                    case "youtube":
                        this.TrailerUrl = $"https://www.youtube.com/embed/{media.Trailer.Id}";
                        break;
                    case "dailymotion":
                        this.TrailerUrl = $"https://www.dailymotion.com/video/{media.Trailer.Id}";
                        break;
                    default:
                        throw new Exception($"Trailer site {media.Trailer.Site} not handled!");
                }
            }

            this.CoverImage = media.CoverImage.Large;
            this.CoverColor = media.CoverImage.Color;
            this.BannerImage = media.BannerImage;
            this.Genres = media.Genres;

            foreach(var tag in media.Tags)
            {
                if (!this.Genres.Contains(tag.Name))
                {
                    this.Genres.Add(tag.Name);
                }
            }

            this.Score = media.AverageScore.HasValue ? media.AverageScore.Value : 0;

            foreach (var edge in media.Relations.Edges)
            {
                if (edge.Type == "SEQUEL" || edge.Type == "PREQUEL")
                {
                    var builder = Builders<Anime>.Filter;
                    FilterDefinition<Anime> filter = builder.Eq("anilist_id", edge.Node.Id);

                    var connected = this._animeCollection.Collection.Find(filter).FirstOrDefault();

                    if(connected != null)
                    {
                        if (edge.Type == "SEQUEL")
                        {
                            this.Sequel = connected.Id;
                        }
                        else
                        {
                            this.Prequel = connected.Id;
                        }
                    }
                }
            }

            if(media.Recommendations.Nodes.Count > 0)
            {
                int recommendationAvgRating = media.Recommendations.Nodes.Sum(x => x.Rating) / media.Recommendations.Nodes.Count;

                foreach (var node in media.Recommendations.Nodes.Where(x => x.Rating > recommendationAvgRating).OrderByDescending(x => x.Rating))
                {
                    if (Recommendations == null)
                    {
                        Recommendations = new List<long>();
                    }

                    var builder = Builders<Anime>.Filter;
                    FilterDefinition<Anime> filter = builder.Eq("anilist_id", node.Media.Id);

                    var recommendation = this._animeCollection.Collection.Find(filter).FirstOrDefault();

                    if (recommendation != null)
                    {
                        Recommendations.Add(recommendation.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Check and return the correct Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private AnilistResponse.MediaDate CheckDate(AnilistResponse.MediaDate date)
        {
            return new AnilistResponse.MediaDate()
            {
                Year = date.Year.HasValue ? date.Year.Value : 1970,
                Month = (date.Month.HasValue ? (date.Month.Value <= 12 ? date.Month.Value : 12) : 1),
                Day = (date.Day.HasValue ? (date.Day.Value <= 31 ? date.Day.Value : 31) : 1)
            };
        }

        [BsonElement("anilist_id")]
        [JsonPropertyName("anilist_id")]
        [JsonProperty(PropertyName = "anilist_id")]
        public int AnilistId { get; set; }

        [BsonElement("mal_id")]
        [JsonPropertyName("mal_id")]
        [JsonProperty(PropertyName = "mal_id")]
        public int? MyAnimeListId { get; set; }

        [BsonElement("tmdb_id")]
        [JsonPropertyName("tmdb_id")]
        [JsonProperty(PropertyName = "tmdb_id")]
        public long? TmdbId { get; set; }

        [BsonElement("format")]
        [JsonPropertyName("format")]
        [JsonProperty(PropertyName = "format")]
        public AnimeFormatEnum Format { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        [JsonProperty(PropertyName = "status")]
        public AnimeStatusEnum Status { get; set; }

        [BsonElement("titles")]
        [JsonPropertyName("titles")]
        [JsonProperty(PropertyName = "titles")]
        public Dictionary<string, string> Titles { get; set; }

        [BsonElement("descriptions")]
        [JsonPropertyName("descriptions")]
        [JsonProperty(PropertyName = "descriptions")]
        public Dictionary<string, string> Descriptions { get; set; }

        [BsonElement("start_date")]
        [JsonPropertyName("start_date")]
        [JsonProperty(PropertyName = "start_date")]
        public DateTime? StartDate { get; set; }

        [BsonElement("end_date")]
        [JsonPropertyName("end_date")]
        [JsonProperty(PropertyName = "end_date")]
        public DateTime? EndDate { get; set; }

        [BsonElement("weekly_airing_day")]
        [JsonPropertyName("weekly_airing_day")]
        [JsonProperty(PropertyName = "weekly_airing_day")]
        public DayOfWeek? WeeklyAiringDay { get; set; }

        [BsonElement("season_period")]
        [JsonPropertyName("season_period")]
        [JsonProperty(PropertyName = "season_period")]
        public AnimeSeasonEnum SeasonPeriod { get; set; }

        [BsonElement("season_year")]
        [JsonPropertyName("season_year")]
        [JsonProperty(PropertyName = "season_year")]
        public int? SeasonYear { get; set; }

        [BsonElement("episodes_count")]
        [JsonPropertyName("episodes_count")]
        [JsonProperty(PropertyName = "episodes_count")]
        public int EpisodesCount { get; set; }

        [BsonElement("episode_duration")]
        [JsonPropertyName("episode_duration")]
        [JsonProperty(PropertyName = "episode_duration")]
        public int? EpisodeDuration { get; set; }

        [BsonElement("trailer_url")]
        [JsonPropertyName("trailer_url")]
        [JsonProperty(PropertyName = "trailer_url")]
        public string TrailerUrl { get; set; }

        [BsonElement("cover_image")]
        [JsonPropertyName("cover_image")]
        [JsonProperty(PropertyName = "cover_image")]
        public string CoverImage { get; set; }

        [BsonElement("cover_color")]
        [JsonPropertyName("cover_color")]
        [JsonProperty(PropertyName = "cover_color")]
        public string CoverColor { get; set; }

        [BsonElement("banner_image")]
        [JsonPropertyName("banner_image")]
        [JsonProperty(PropertyName = "banner_image")]
        public string BannerImage { get; set; }

        [BsonElement("genres")]
        [JsonPropertyName("genres")]
        [JsonProperty(PropertyName = "genres")]
        public List<string> Genres { get; set; }

        [BsonElement("sagas")]
        [JsonPropertyName("sagas")]
        [JsonProperty(PropertyName = "sagas")]
        public List<Saga> Sagas { get; set; }

        [BsonElement("sequel")]
        [JsonPropertyName("sequel")]
        [JsonProperty(PropertyName = "sequel")]
        public long? Sequel { get; set; }

        [BsonElement("prequel")]
        [JsonPropertyName("prequel")]
        [JsonProperty(PropertyName = "prequel")]
        public long? Prequel { get; set; }

        [BsonElement("score")]
        [JsonPropertyName("score")]
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        [BsonElement("has_episodes")]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasEpisodes { get; set; } = false;

        [BsonElement("recommendations")]
        [JsonPropertyName("recommendations")]
        [JsonProperty(PropertyName = "recommendations")]
        public List<long> Recommendations { get; set; }

        [JsonPropertyName("nsfw")]
        [JsonProperty(PropertyName = "nsfw")]
        public bool NSFW => Genres.Contains("Hentai") || Genres.Contains("Nudity");

        [JsonPropertyName("has_cover_image")]
        [JsonProperty(PropertyName = "has_cover_image")]
        public bool HasCoverImage => CoverImage != "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/default.jpg";

        public override string ToString()
        {
            return this.Titles[LocalizationEnum.Romaji];
        }

        public class Saga
        {
            [BsonElement("titles")]
            [JsonPropertyName("titles")]
            [JsonProperty(PropertyName = "titles")]
            public Dictionary<string, string> Titles { get; set; }

            [BsonElement("descriptions")]
            [JsonPropertyName("descriptions")]
            [JsonProperty(PropertyName = "descriptions")]
            public Dictionary<string, string> Descriptions { get; set; }

            [BsonElement("episode_from")]
            [JsonPropertyName("episode_from")]
            [JsonProperty(PropertyName = "episode_from")]
            public int EpisodeFrom { get; set; }

            [BsonElement("episode_to")]
            [JsonPropertyName("episode_to")]
            [JsonProperty(PropertyName = "episode_to")]
            public int EpisodeTo { get; set; }

            [BsonElement("episodes_count")]
            [JsonPropertyName("episodes_count")]
            [JsonProperty(PropertyName = "episodes_count")]
            public int EpisodesCount { get; set; }
        }
    }
}
