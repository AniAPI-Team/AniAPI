using MongoDB.Bson.Serialization.Attributes;
using MongoService;

namespace Models
{
    public class ServicesStatus : IModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("status")]
        public ServiceStatusEnum Status { get; set; }
    }
}
