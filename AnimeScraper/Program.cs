using AnimeScraper.Helpers;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Models;
using MongoService;
using MongoService.Models;
using System;
using System.Collections.Generic;

namespace AnimeScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            //Anime a = c.Get(1);
            //List<Anime> a = c.GetList(new AnimeFilter() { Title = "Agame", Locale = LocalizationEnum.English });
            //Anime a = new Anime(LocalizationEnum.English, "Agame Ga Kill", "I am a provissory description");
            //c.Add(ref a);

            ScraperEngine.Instance.Start();
            Console.ReadLine();
        }
    }
}
