using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commons
{
    public class AnimeEpisodi
    {

        [BsonElement("mal_id")]
        public int? MyAnimeListId { get; set; }
        public List<Episodio> Episodi { get; set;}
    }

    public class Episodio
    {
        public int NumeroEpisodio { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Sorgente { get; set; }
    }
}
