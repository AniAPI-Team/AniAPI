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
    public class AnimeSuggestion : IModel
    {
        public AnimeSuggestion() { }

        [BsonElement("anime_id")]
        [JsonPropertyName("anime_id")]
        [JsonProperty(PropertyName = "anime_id")]
        public long AnimeID { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [BsonElement("source")]
        [JsonPropertyName("source")]
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [BsonElement("score")]
        [JsonPropertyName("score")]
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        [BsonElement("path")]
        [JsonPropertyName("path")]
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        [JsonProperty(PropertyName = "status")]
        public AnimeSuggestionStatusEnum Status { get; set; }
    }
}
