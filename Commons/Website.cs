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
    public class Website : IModel
    {
        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [BsonElement("official")]
        [JsonPropertyName("official")]
        public bool Official { get; set; }

        [BsonElement("site_url")]
        [JsonPropertyName("site_url")]
        public string SiteUrl { get; set; }

        [BsonElement("search")]
        [JsonPropertyName("search")]
        public WebsiteSearch Search { get; set; }

        [BsonElement("localization")]
        [JsonPropertyName("localization")]
        public string Localization { get; set; }
    }
}
