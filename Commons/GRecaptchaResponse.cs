using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class GRecaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public string ChallengeTimestamp { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
