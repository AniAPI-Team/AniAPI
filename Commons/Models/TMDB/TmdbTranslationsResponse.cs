using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class TmdbTranslationsResponse
    {
        [JsonProperty("translations")]
        public List<ResponseResult> Translations { get; set; }

        public class ResponseResult
        {
            [JsonProperty("iso_639_1")]
            public string Iso639 { get; set; }

            [JsonProperty("data")]
            public ResultData Data { get; set; }
        }

        public class ResultData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("overview")]
            public string Overview { get; set; }
        }
    }
}
