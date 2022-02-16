using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class TmdbAlternativeTitlesResponse
    {
        [JsonProperty("results")]
        public List<ResponseResult> Results { get; set; }

        public class ResponseResult
        {
            [JsonProperty("iso_3166_1")]
            public string Iso3166 { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }
        }
    }
}
