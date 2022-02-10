using Commons.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class AnilistImportResponse
    {
        [JsonProperty("data")]
        public ResponseData Data { get; set; }

        public class ResponseData
        {
            public ResponseMediaListCollection MediaListCollection { get; set; }
        }

        public class ResponseMediaListCollection
        {
            [JsonProperty("lists")]
            public List<ResponseMediaList> Lists { get; set; }
        }

        public class ResponseMediaList
        {
            [JsonProperty("isCustomList")]
            public bool IsCustomList { get; set; }

            [JsonProperty("entries")]
            public List<ResponseMedia> Entries { get; set; }
        }

        public class ResponseMedia
        {
            [JsonProperty("mediaId")]
            public int MediaId { get; set; }

            [JsonProperty("status")]
            public UserStoryStatusEnum Status { get; set; }

            [JsonProperty("progress")]
            public int Progress { get; set; }
        }
    }
}
