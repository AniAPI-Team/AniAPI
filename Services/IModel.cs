using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    public abstract class IModel
    {
        [BsonElement("_id")]
        public long Id { get; set; }
        
        [BsonElement("creation_date")]
        public DateTime CreationDate { get; set; }

        [BsonElement("update_date")]
        public DateTime? UpdateDate { get; set; }
    }
}
