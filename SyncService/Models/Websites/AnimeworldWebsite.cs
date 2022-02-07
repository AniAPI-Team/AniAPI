using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models.Websites
{
    public class AnimeworldWebsite : IWebsite
    {
        public AnimeworldWebsite(Website website) : base(website)
        {
        }

        public override bool AnalyzeMatching(Anime anime, AnimeMatching matching, string sourceTitle)
        {
            if(matching.Title.ToLower() == $"{sourceTitle.ToLower()} (ita)")
            {
                matching.IsDub = true;
                return true;
            }

            return base.AnalyzeMatching(anime, matching, sourceTitle);
        }

        public override string BuildAPIProxyURL(AppSettings settings, AnimeMatching matching, string url, Dictionary<string, string> values = null)
        {
            return url;
        }
    }
}
