using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Commons.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commons
{
    public class AnilistResponse
    {
        [JsonProperty("data")]
        public ResponseData Data { get; set; }

        public class ResponseData
        {
            public ResponsePage Page { get; set; }
        }

        public class ResponsePage
        {
            [JsonProperty("pageInfo")]
            public ResponsePageInfo PageInfo { get; set; }

            [JsonProperty("media")]
            public List<ResponseMedia> Media { get; set; }
        }

        public class ResponsePageInfo
        {
            [JsonProperty("lastPage")]
            public int LastPage { get; set; }
        }

        public class ResponseMedia
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("idMal")]
            public int? IdMal { get; set; }

            [JsonProperty("format")]
            public AnimeFormatEnum Format { get; set; }

            [JsonProperty("status")]
            public AnimeStatusEnum? Status { get; set; }

            [JsonProperty("startDate")]
            public MediaDate StartDate { get; set; }

            [JsonProperty("endDate")]
            public MediaDate EndDate { get; set; }

            [JsonProperty("title")]
            public MediaTitle Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("season")]
            public AnimeSeasonEnum? Season { get; set; }

            [JsonProperty("seasonYear")]
            public int? SeasonYear { get; set; }

            [JsonProperty("episodes")]
            public int? Episodes { get; set; }

            [JsonProperty("duration")]
            public int? Duration { get; set; }

            [JsonProperty("trailer")]
            public MediaTrailer Trailer { get; set; }

            [JsonProperty("coverImage")]
            public MediaCoverImage CoverImage { get; set; }

            [JsonProperty("bannerImage")]
            public string BannerImage { get; set; }

            [JsonProperty("averageScore")]
            public int? AverageScore { get; set; }

            [JsonProperty("genres")]
            public List<string> Genres { get; set; }

            [JsonProperty("tags")]
            public List<MediaTag> Tags { get; set; }

            [JsonProperty("relations")]
            public MediaRelations Relations { get; set; }

            [JsonProperty("nextAiringEpisode")]
            public MediaAiringEpisode? AiringEpisode { get; set; }

            [JsonProperty("recommendations")]
            public MediaRecommendation Recommendations { get; set; }
        }

        public class MediaTitle
        {
            [JsonProperty("romaji")]
            public string Romaji { get; set; }

            [JsonProperty("english")]
            public string English { get; set; }

            [JsonProperty("native")]
            public string Native { get; set; }
        }

        public class MediaDate
        {
            [JsonProperty("year")]
            public int? Year { get; set; }

            [JsonProperty("month")]
            public int? Month { get; set; }

            [JsonProperty("day")]
            public int? Day { get; set; }
        }

        public class MediaTrailer
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("site")]
            public string Site { get; set; }
        }

        public class MediaCoverImage
        {
            [JsonProperty("large")]
            public string Large { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }
        }

        public class MediaTag
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class MediaRelations
        {
            [JsonProperty("edges")]
            public List<MediaRelationsEdge> Edges { get; set; }
        }

        public class MediaRelationsEdge
        {
            [JsonProperty("node")]
            public MediaRelationsNode Node { get; set; }

            [JsonProperty("relationType")]
            public string Type { get; set; }
        }

        public class MediaRelationsNode
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }
        }

        public class MediaAiringEpisode
        {
            [JsonProperty("episode")]
            public int Episode { get; set; }
        }

        public class MediaRecommendation
        {
            [JsonProperty("nodes")]
            public List<MediaRecommendationNode> Nodes { get; set; }
        }

        public class MediaRecommendationNode
        {
            [JsonProperty("rating")]
            public int Rating { get; set; }

            [JsonProperty("mediaRecommendation")]
            public MediaRecommendationMedia Media { get; set; }
        }

        public class MediaRecommendationMedia
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }
    }
}
