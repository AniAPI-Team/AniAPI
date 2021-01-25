using MongoDB.Bson.Serialization.Attributes;
using MongoService;
using Commons.Enums;

namespace Commons
{
    public class ServicesStatus : IModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("status")]
        public ServiceStatusEnum Status { get; set; }
    }
}
