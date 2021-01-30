using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons
{
    public class AnimeSong
    {
        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [BsonElement("artist")]
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [BsonElement("album")]
        [JsonPropertyName("album")]
        public string Album { get; set; }

        [BsonElement("year")]
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [BsonElement("season")]
        [JsonPropertyName("season")]
        public string Season { get; set; }

        [BsonElement("duration")]
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [BsonElement("preview_url")]
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }

        [BsonElement("spotify_url")]
        [JsonPropertyName("spotify_url")]
        public string SpotifyUrl { get; set; }

        public void SetSeason(int month)
        {
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    this.Season = "Winter";
                    break;
                case 4:
                case 5:
                case 6:
                    this.Season = "Spring";
                    break;
                case 7:
                case 8:
                case 9:
                    this.Season = "Summer";
                    break;
                case 10:
                case 11:
                case 12:
                    this.Season = "Fall";
                    break;
            }
        }
    }
}
