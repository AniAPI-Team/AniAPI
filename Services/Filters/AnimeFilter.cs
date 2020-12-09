using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Filters
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        public string Title { get; set; }
        public int AnilistId { get; set; }
    }
}
