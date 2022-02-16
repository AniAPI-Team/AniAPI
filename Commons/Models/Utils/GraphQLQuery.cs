using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commons
{
    public class GraphQLQuery
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("variables")]
        public Dictionary<string, object> Variables { get; set; }
    }
}
