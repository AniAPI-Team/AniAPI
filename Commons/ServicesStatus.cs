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

        [BsonElement("progress")]
        public double Progress { get; set; }

        public ServicesStatus() { }

        public ServicesStatus(string name)
        {
            this.Name = name;
            this.Status = ServiceStatusEnum.NONE;
            this.Progress = 0;
        }
    }
}
