using SyncService.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService
{
    public static class WorkerPool
    {
        public static AnimeScraperWorker AnimeScraperWorker => new AnimeScraperWorker();
        public static WebsiteScraperWorker WebsiteScraperWorker => new WebsiteScraperWorker();
        public static SongScraperWorker SongScraperWorker => new SongScraperWorker();
        public static UserSyncWorker UserSyncWorker => new UserSyncWorker();
    }
}
