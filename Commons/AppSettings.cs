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
    }
}
