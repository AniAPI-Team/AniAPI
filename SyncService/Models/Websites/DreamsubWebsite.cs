using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models.Websites
{
    public class DreamsubWebsite : IWebsite
    {
        public DreamsubWebsite(Website website) : base(website)
        {
        }

        public override string BuildAPIProxyURL(AppSettings settings, AnimeMatching matching, string url, Dictionary<string, string> values = null)
        {
            values = new Dictionary<string, string>
            {
                { "host", "cdn.dreamsub.cc" },
                { "referer", matching.EpisodePath }
            };

            return base.BuildAPIProxyURL(settings, matching, url, values);
        }
    }
}
