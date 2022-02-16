using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class TmdbSearchMovieResponse
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("results")]
        public List<ResponseResult> Results { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        public class ResponseResult
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("title")]
            public string Name { get; set; }

            [JsonProperty("release_date")]
            public DateTime? ReleaseDate { get; set; }
        }
    }
}
