using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models
{
    public class AnimeMatching
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
        public string Path { get; set; }
        public List<EpisodeMatching> Episodes { get; set; } = new List<EpisodeMatching>();
    }
}
