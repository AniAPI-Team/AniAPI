using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MongoService
{
    /// <summary>
    /// Base class to create a MongoDB document model
    /// </summary>
    public abstract class IModel
    {
        /// <summary>
        /// The document id
        /// </summary>
        [BsonElement("_id")]
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// The document creation time
        /// </summary>
        [BsonElement("creation_date")]
        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The document last update time
        /// </summary>
        [BsonElement("update_date")]
        [JsonIgnore]
        public DateTime? UpdateDate { get; set; }
    }
}
