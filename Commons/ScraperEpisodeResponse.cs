using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class ScraperEpisodeResponse
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

            [JsonProperty("url")]
            public string Video { get; set; }

            [JsonProperty("quality")]
            public string Quality { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }
        }
    }
}
