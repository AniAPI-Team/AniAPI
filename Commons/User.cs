using Commons.Enums;
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
    public class User : IModel
    {
        [BsonElement("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [BsonElement("gender")]
        [JsonPropertyName("gender")]
        public UserGenderEnum Gender { get; set; }

        [BsonElement("localization")]
        [JsonPropertyName("localization")]
        public string Localization { get; set; }
    }
}
