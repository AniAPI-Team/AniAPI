using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;



namespace SyncService.Models.Websites
{
    class IAnimeInfo {
        public string title { get; set; }
        public int episodeNumber { get; set; }
        public bool IsDub { get; set; }
    }
    public class AnimepisodeWebsite : IWebsite
    {
        private IAnimeInfo FixTitle(string title)
        {
            // Remove all numbers from the title using regexp
            Regex rx = new Regex(@"(\d+)(?!.*\d)");
            
            // Use the regex "rx" to remove the last number in title and store it in "episodeNumber"
            int episodeNumber = int.Parse(rx.Match(title).Value);
            title = rx.Replace(title, "");

            // Check if the title contains "Dub" or "Dubbed"
            bool IsDub = title.Contains("Dubbed");

            // Remove "Dubbed" and "Subbed" from the title
            title = title.Replace("Dubbed", "");
            title = title.Replace("Subbed", "");
            // Remove "Episode" and "English" from the title
            title = title.Replace("Episode", "");
            title = title.Replace("English", "");

            // Return the title, episode number and if it is a dub
            return new IAnimeInfo() { title = title, episodeNumber = episodeNumber, IsDub = IsDub };
            
            
        }
        public AnimepisodeWebsite(Website website) : base(website)
        {
        }

        public override bool AnalyzeMatching(Anime anime, AnimeMatching matching, string sourceTitle)
        {
            
         
            IAnimeInfo animeInfo = FixTitle(sourceTitle);
            matching.IsDub = animeInfo.IsDub;
            return base.AnalyzeMatching(anime, matching, animeInfo.title);
        }

        public override Dictionary<string, string> GetVideoProxyHeaders(AnimeMatching matching, Dictionary<string, string> values = null)
        {
            return null;
        }
    }
}
