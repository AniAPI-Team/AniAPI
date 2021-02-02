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

        [BsonIgnore]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [BsonElement("password_hash")]
        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; }

        [BsonElement("email_verified")]
        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [BsonElement("last_login_date")]
        [JsonPropertyName("last_login_date")]
        public DateTime? LastLoginDate { get; set; }

        [BsonIgnore]
        [JsonPropertyName("access_token")]
        public string Token { get; set; }

        [BsonElement("role")]
        [JsonPropertyName("role")]
        public UserRoleEnum Role { get; set; }

        [BsonElement("gender")]
        [JsonPropertyName("gender")]
        public UserGenderEnum Gender { get; set; }

        [BsonElement("localization")]
        [JsonPropertyName("localization")]
        public string Localization { get; set; }

        public void HideConfindentialValues()
        {
            this.Password = null;
            this.PasswordHash = null;
            this.Email = null;
            this.EmailVerified = null;
            this.LastLoginDate = null;
            this.Token = null;
            this.Localization = null;
        }
    }
}
