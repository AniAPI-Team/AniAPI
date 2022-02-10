using Commons.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons
{
    public class AnimeSong : IModel
    {
        [BsonElement("anime_id")]
        [JsonPropertyName("anime_id")]
        [JsonProperty(PropertyName = "anime_id")]
        public long AnimeID { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [BsonElement("artist")]
        [JsonPropertyName("artist")]
        [JsonProperty(PropertyName = "artist")]
        public string Artist { get; set; }

        [BsonElement("album")]
        [JsonPropertyName("album")]
        [JsonProperty(PropertyName = "album")]
        public string Album { get; set; }

        [BsonElement("year")]
        [JsonPropertyName("year")]
        [JsonProperty(PropertyName = "year")]
        public int Year { get; set; }

        [BsonElement("season")]
        [JsonPropertyName("season")]
        [JsonProperty(PropertyName = "season")]
        public AnimeSeasonEnum Season { get; set; }

        [BsonElement("duration")]
        [JsonPropertyName("duration")]
        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }

        [BsonElement("preview_url")]
        [JsonPropertyName("preview_url")]
        [JsonProperty(PropertyName = "preview_url")]
        public string PreviewUrl { get; set; }

        [BsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string SpotifyID { get; set; }

        [BsonElement("open_spotify_url")]
        [JsonPropertyName("open_spotify_url")]
        [JsonProperty(PropertyName = "open_spotify_url")]
        public string OpenSpotifyUrl { get; set; }

        [BsonElement("local_spotify_url")]
        [JsonPropertyName("local_spotify_url")]
        [JsonProperty(PropertyName = "local_spotify_url")]
        public string LocalSpotifyUrl { get; set; }

        [BsonElement("type")]
        [JsonPropertyName("type")]
        [JsonProperty(PropertyName = "type")]
        public AnimeSongTypeEnum SongType { get; set; }

        public void SetSeason(int month)
        {
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    this.Season = AnimeSeasonEnum.WINTER;
                    break;
                case 4:
                case 5:
                case 6:
                    this.Season = AnimeSeasonEnum.SPRING;
                    break;
                case 7:
                case 8:
                case 9:
                    this.Season = AnimeSeasonEnum.SUMMER;
                    break;
                case 10:
                case 11:
                case 12:
                    this.Season = AnimeSeasonEnum.FALL;
                    break;
            }
        }
    }
}
