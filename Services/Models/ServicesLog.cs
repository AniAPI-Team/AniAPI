using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Models
{
    public class ServicesLog : IModel
    {
        [BsonElement("service_id")]
        public long ServiceId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }
    }
}
