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
    public class User : IModel
    {
        [BsonElement("username")]
        [JsonPropertyName("username")]
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [BsonIgnore]
        [JsonPropertyName("password")]
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [BsonElement("password_hash")]
        [JsonPropertyName("password_hash")]
        [JsonProperty(PropertyName = "password_hash")]
        public string PasswordHash { get; set; }

        [BsonElement("email_verified")]
        [JsonPropertyName("email_verified")]
        [JsonProperty(PropertyName = "email_verified")]
        public bool? EmailVerified { get; set; }

        [BsonElement("last_login_date")]
        [JsonPropertyName("last_login_date")]
        [JsonProperty(PropertyName = "last_login_date")]
        public DateTime? LastLoginDate { get; set; }

        [BsonIgnore]
        [JsonPropertyName("access_token")]
        [JsonProperty(PropertyName = "access_token")]
        public string Token { get; set; }

        [BsonElement("role")]
        [JsonPropertyName("role")]
        [JsonProperty(PropertyName = "role")]
        public UserRoleEnum Role { get; set; }

        [BsonElement("gender")]
        [JsonPropertyName("gender")]
        [JsonProperty(PropertyName = "gender")]
        public UserGenderEnum Gender { get; set; }

        [BsonElement("avatar")]
        [JsonPropertyName("avatar")]
        [JsonProperty(PropertyName = "avatar")]
        public string Avatar { get; set; }

        [BsonElement("avatar_tracker")]
        [JsonPropertyName("avatar_tracker")]
        [JsonProperty(PropertyName = "avatar_tracker")]
        public string AvatarTracker { get; set; }

        [BsonElement("localization")]
        [JsonPropertyName("localization")]
        [JsonProperty(PropertyName = "localization")]
        public string Localization { get; set; }

        [BsonElement("anilist_id")]
        [JsonPropertyName("anilist_id")]
        [JsonProperty(PropertyName = "anilist_id")]
        public long? AnilistId { get; set; }

        [BsonElement("anilist_token")]
        [JsonPropertyName("anilist_token")]
        [JsonProperty(PropertyName = "anilist_token")]
        public string AnilistToken { get; set; }

        [JsonPropertyName("has_anilist")]
        [JsonProperty(PropertyName = "has_anilist")]
        [BsonIgnore]
        public bool? HasAnilist { get; set; } = null;

        [BsonElement("mal_id")]
        [JsonPropertyName("mal_id")]
        [JsonProperty(PropertyName = "mal_id")]
        public long? MyAnimeListId { get; set; }

        [BsonElement("mal_token")]
        [JsonPropertyName("mal_token")]
        [JsonProperty(PropertyName = "mal_token")]
        public string MyAnimeListToken { get; set; }

        [JsonPropertyName("has_mal")]
        [JsonProperty(PropertyName = "has_mal")]
        [BsonIgnore]
        public bool? HasMyAnimeList { get; set; } = null;

        public void CalcDerivedFields()
        {
            HasAnilist = AnilistId != null && !string.IsNullOrEmpty(AnilistToken);
            HasMyAnimeList = MyAnimeListId != null && !string.IsNullOrEmpty(MyAnimeListToken);
        }

        public void HideConfindentialValues()
        {
            this.Password = null;
            this.PasswordHash = null;
            this.Email = null;
            this.EmailVerified = null;
            this.LastLoginDate = null;
            this.Token = null;
            this.Localization = null;
            this.AnilistId = null;
            this.AnilistToken = null;
            this.HasAnilist = null;
            this.MyAnimeListId = null;
            this.MyAnimeListToken = null;
            this.HasMyAnimeList = null;
            this.AvatarTracker = null;
        }
    }
}
