using AnimeScraper.Helpers;
using System;

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
