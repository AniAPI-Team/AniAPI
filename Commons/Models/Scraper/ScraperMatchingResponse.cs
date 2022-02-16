using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class ScraperMatchingResponse
    {
        [JsonProperty("size")]
        public double Size { get; set; }

        [JsonProperty("data")]
        public List<ResponseData> Data { get; set; }

        public class ResponseData
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }
        }
    }
}
