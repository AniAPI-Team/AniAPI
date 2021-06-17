using Commons.Enums;
using MongoService;
using System.Collections.Generic;

namespace Commons.Filters
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        public string title { get; set; }
        public int? anilist_id { get; set; }
        public int? mal_id { get; set; }
        public List<AnimeFormatEnum> formats { get; set; } = new List<AnimeFormatEnum>();
        public AnimeStatusEnum? status { get; set; }
        public int? year { get; set; }
        public AnimeSeasonEnum? season { get; set; }
        public List<string> genres { get; set; } = new List<string>();
    }
}
