using MongoDB.Bson.Serialization.Attributes;
using MongoService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commons
{
    public class AppSettings : IModel
    {
        [BsonElement("jwt_secret")]
        public string JWTSecret { get; set; }

        [BsonElement("recaptcha_secret")]
        public string RecaptchaSecret { get; set; }

        [BsonElement("proxy_host")]
        public string ProxyHost { get; set; }

        [BsonElement("proxy_port")]
        public string ProxyPort { get; set; }

        [BsonElement("proxy_username")]
        public string ProxyUsername { get; set; }

        [BsonElement("proxy_password")]
        public string ProxyPassword { get; set; }

        [BsonElement("proxy_count")]
        public int ProxyCount { get; set; }

        [BsonElement("resources_version")]
        public string ResourcesVersion { get; set; }

        [BsonElement("api_endpoint")]
        public string APIEndpoint { get; set; }

        [BsonElement("smtp")]
        public SmtpConfig Smtp { get; set; }

        [BsonElement("mal")]
        public MyAnimeListConfig MyAnimeList { get; set; }

        [BsonElement("apilytics_key")]
        public string ApilyticsKey { get; set; }

        [BsonElement("tmdb_key")]
        public string TmdbKey { get; set; }

        public class SmtpConfig
        {
            [BsonElement("host")]
            public string Host { get; set; }

            [BsonElement("port")]
            public int Port { get; set; }

            [BsonElement("username")]
            public string Username { get; set; }

            [BsonElement("password")]
            public string Password { get; set; }

            [BsonElement("address")]
            public string Address{ get; set; }
        }

        public class MyAnimeListConfig
        {
            [BsonElement("client_id")]
            public string ClientID { get; set; }

            [BsonElement("client_secret")]
            public string ClientSecret { get; set; }
        }
    }
}
