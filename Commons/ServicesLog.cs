using MongoDB.Bson.Serialization.Attributes;
using MongoService;

namespace Commons
{
    public class ServicesLog : IModel
    {
        [BsonElement("service_id")]
        public long ServiceId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }
    }
}
