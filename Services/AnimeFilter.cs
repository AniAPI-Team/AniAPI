using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    public class AnimeFilter : IFilter<AnimeFilter>
    {
        public string Title { get; set; }
    }
}
