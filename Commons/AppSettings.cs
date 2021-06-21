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
    }
}
