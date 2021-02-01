using MongoService;

namespace Commons.Filters
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        public string title { get; set; }
        public int anilist_id { get; set; }
    }
}
