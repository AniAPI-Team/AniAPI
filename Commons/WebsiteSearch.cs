using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons
{
    public class WebsiteSearch
    {
        [BsonElement("query")]
        [JsonPropertyName("query")]
        public string Query { get; set; }

        [BsonElement("wait_selector")]
        [JsonPropertyName("wait_selector")]
        public string WaitSelector { get; set; }

        [BsonElement("elements_selector")]
        [JsonPropertyName("elements_selector")]
        public string ElementsSelector { get; set; }

        [BsonElement("title_selector")]
        [JsonPropertyName("title_selector")]
        public string TitleSelector { get; set; }

        [BsonElement("title_function")]
        [JsonPropertyName("title_function")]
        public string TitleFunction { get; set; }
    }
}
