using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class AnilistAvatarResponse
    {
        [JsonProperty("data")]
        public ResponseData Data { get; set; }

        public class ResponseData
        {
            public ResponseUser User { get; set; }
        }

        public class ResponseUser
        {
            [JsonProperty("avatar")]
            public ResponseUserAvatar Avatar { get; set; }
        }

        public class ResponseUserAvatar
        {
            [JsonProperty("large")]
            public string Large { get; set; }
        }
    }
}
