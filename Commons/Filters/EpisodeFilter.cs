using MongoService;

namespace Commons.Filters
{
    public class EpisodeFilter : IFilter<EpisodeFilter>
    {
        public long anime_id { get; set; }
        public int number { get; set; }
        public string source { get; set; }
    }
}
