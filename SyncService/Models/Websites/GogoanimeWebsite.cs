using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models.Websites
{
    public class GogoanimeWebsite : IWebsite
    {
        public GogoanimeWebsite(Website website) : base(website)
        {
        }

        public override bool AnalyzeMatching(Anime anime, AnimeMatching matching, string sourceTitle)
        {
            if(matching.Title.ToLower() == $"{sourceTitle.ToLower()} (dub)")
            {
                matching.IsDub = true;
                return true;
            }

            return base.AnalyzeMatching(anime, matching, sourceTitle);
        }

        public override string BuildAPIProxyURL(AppSettings settings, AnimeMatching matching, string url, Dictionary<string, string> values = null)
        {
            values = new Dictionary<string, string>()
            {
                { "referer", matching.EpisodePath }
            };

            return base.BuildAPIProxyURL(settings, matching, url, values);
        }
    }
}
