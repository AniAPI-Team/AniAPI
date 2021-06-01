using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Filters
{
    public class AnimeSuggestionFilter : IFilter<AnimeSuggestionFilter>
    {
        public long? anime_id { get; set; }
        public string title { get; set; }
        public string source { get; set; }
    }
}
