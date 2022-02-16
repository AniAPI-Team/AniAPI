using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class TmdbSeasonTvResponse
    {
        [JsonProperty("episodes")]
        public List<SeasonEpisode> Episodes { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("overview")]
        public string Description { get; set; }

        public class SeasonEpisode
        {
            [JsonProperty("episode_number")]
            public int EpisodeNumber { get; set; }
        }
    }
}
