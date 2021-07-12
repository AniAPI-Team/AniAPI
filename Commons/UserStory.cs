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
    public class UserStory : IModel
    {
        [BsonElement("user_id")]
        [JsonPropertyName("user_id")]
        [JsonProperty(PropertyName = "user_id")]
        public long UserID { get; set; }

        [BsonElement("anime_id")]
        [JsonPropertyName("anime_id")]
        [JsonProperty(PropertyName = "anime_id")]
        public long AnimeID { get; set; }

        [BsonElement("status")]
        [JsonPropertyName("status")]
        [JsonProperty(PropertyName = "status")]
        public UserStoryStatusEnum Status { get; set; }

        [BsonElement("current_episode")]
        [JsonPropertyName("current_episode")]
        [JsonProperty(PropertyName = "current_episode")]
        public int CurrentEpisode { get; set; }

        [BsonElement("current_episode_ticks")]
        [JsonPropertyName("current_episode_ticks")]
        [JsonProperty(PropertyName = "current_episode_ticks")]
        public long? CurrentEpisodeTicks { get; set; }

        [BsonElement("synced")]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool Synced { get; set; }
    }
}
