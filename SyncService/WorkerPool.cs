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
    }
}
