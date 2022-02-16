using Commons.Enums;
using MongoService;
using ServiceMongo.Attributes;
using System.Collections.Generic;

namespace Commons.Filters
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        public string title { get; set; }
        public int? anilist_id { get; set; }
        public int? mal_id { get; set; }
        public long? tmbd_id { get; set; }

        [CommaSeparated]
        public List<AnimeFormatEnum> formats { get; set; } = new List<AnimeFormatEnum>();
        
        public AnimeStatusEnum? status { get; set; }
        public int? year { get; set; }
        public AnimeSeasonEnum? season { get; set; }

        [CommaSeparated]
        public List<string> genres { get; set; } = new List<string>();

        public bool nsfw { get; set; } = false;

        public bool with_episodes { get; set; } = false;
    }
}
