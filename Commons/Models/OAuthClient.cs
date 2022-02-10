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
    public class OAuthClient : IModel
    {
        [BsonElement("user_id")]
        [JsonPropertyName("user_id")]
        [JsonProperty(PropertyName = "user_id")]
        public long UserID { get; set; }

        [BsonElement("client_id")]
        [JsonPropertyName("client_id")]
        [JsonProperty(PropertyName = "client_id")]
        public Guid ClientID { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [BsonElement("client_secret")]
        [JsonPropertyName("client_secret")]
        [JsonProperty(PropertyName = "client_secret")]
        public Guid ClientSecret { get; set; }

        [BsonElement("redirect_uri")]
        [JsonPropertyName("redirect_uri")]
        [JsonProperty(PropertyName = "redirect_uri")]
        public string RedirectURI { get; set; }

        [BsonElement("is_unlimited")]
        public bool IsUnlimited { get; set; }
    }
}
