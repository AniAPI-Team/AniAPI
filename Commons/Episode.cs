using MongoDB.Bson.Serialization.Attributes;
using MongoService;
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
        public long AnimeID { get; set; }

        [BsonElement("number")]
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [BsonElement("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [BsonElement("video")]
        [JsonPropertyName("video")]
        public string Video { get; set; }

        [BsonElement("source")]
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
