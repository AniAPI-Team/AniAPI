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
    public class Episode : IModel
    {
        [BsonElement("anime_id")]
        [JsonPropertyName("anime_id")]
        [JsonProperty(PropertyName = "anime_id")]
        public long AnimeID { get; set; }

        [BsonElement("number")]
        [JsonPropertyName("number")]
        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [BsonElement("video")]
        [JsonPropertyName("video")]
        [JsonProperty(PropertyName = "video")]
        public string Video { get; set; }

        [BsonElement("video_headers")]
        [JsonPropertyName("video_headers")]
        [JsonProperty(PropertyName = "video_headers")]
        public Dictionary<string, string> VideoHeaders { get; set; }

        [BsonElement("quality")]
        [JsonPropertyName("quality")]
        [JsonProperty(PropertyName = "quality")]
        public string Quality { get; set; }

        [BsonElement("format")]
        [JsonPropertyName("format")]
        [JsonProperty(PropertyName = "format")]
        public string Format { get; set; }

        [BsonElement("source")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Source { get; set; }

        [BsonElement("locale")]
        [JsonPropertyName("locale")]
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [BsonElement("is_dub")]
        [JsonPropertyName("is_dub")]
        [JsonProperty(PropertyName = "is_dub")]
        public bool IsDub { get; set; }
    }
}
