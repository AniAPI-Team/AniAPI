using Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Models
{
    public class ServicesStatus : IModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("status")]
        public ServiceStatusEnum Status { get; set; }
    }
}
