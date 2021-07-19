using Commons.Enums;
using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Filters
{
    public class AnimeSongFilter : IFilter<AnimeSongFilter>
    {
        public long? anime_id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public int? year { get; set; }
        public AnimeSeasonEnum? season { get; set; }
        public AnimeSongTypeEnum? type { get; set; }
    }
}
