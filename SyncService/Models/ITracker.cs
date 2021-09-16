using Commons;
using Commons.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models
{
    public abstract class ITracker
    {
        protected AnimeCollection _animeCollection = new AnimeCollection();
        protected HttpClient client;

        public Anime Anime { get; set; }
        public User User { get; set; }
        public bool HasDone { get; protected set; }

        public abstract string Name { get; }

        public ITracker(HttpClient client)
        {
            this.client = client;
        }

        public abstract Task Export(UserStory s);
        public abstract Task<string> GetAvatar();
        public abstract Task<List<UserStory>> Import();
        public abstract bool NeedWork();
    }
}
