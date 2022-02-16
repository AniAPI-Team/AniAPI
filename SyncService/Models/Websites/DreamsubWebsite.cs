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

        public override Dictionary<string, string> GetVideoProxyHeaders(AnimeMatching matching, Dictionary<string, string> values = null)
        {
            return new Dictionary<string, string>
            {
                { "host", "cdn.dreamsub.cc" },
                { "referer", matching.EpisodePath }
            };
        }
    }
}
