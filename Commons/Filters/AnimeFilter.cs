using Microsoft.AspNetCore.Mvc;
using MongoService;

namespace Commons.Filters
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        [FromQuery(Name = "title")]
        public string Title { get; set; }

        [FromQuery(Name = "anilist_id")]
        public int AnilistId { get; set; }
    }
}
