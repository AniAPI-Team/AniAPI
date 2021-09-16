using Commons.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class MyAnimeListImportResponse
    {
        [JsonProperty("data")]
        public List<ResponseData> Data { get; set; }

        [JsonProperty("paging")]
        public ResponsePaging Paging { get; set; }

        public class ResponseData
        {
            [JsonProperty("node")]
            public ResponseNode Node { get; set; }

            [JsonProperty("list_status")]
            public ResponseStatus Status { get; set; }

            public UserStoryStatusEnum InternalStatus
            {
                get
                {
                    switch (Status.Status)
                    {
                        case "watching": return UserStoryStatusEnum.CURRENT;
                        case "plan_to_watch": return UserStoryStatusEnum.PLANNING;
                        case "completed": return UserStoryStatusEnum.COMPLETED;
                        case "dropped": return UserStoryStatusEnum.DROPPED;
                        case "on_hold": return UserStoryStatusEnum.PAUSED;
                    }

                    return UserStoryStatusEnum.REPEATING;
                }
            }
        }

        public class ResponseNode
        {
            [JsonProperty("id")]
            public int AnimeID { get; set; }
        }

        public class ResponseStatus
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("num_episodes_watched")]
            public int EpisodesWatched { get; set; }
        }

        public class ResponsePaging
        {
            [JsonProperty("next")]
            public string Next { get; set; }
        }
    }
}
