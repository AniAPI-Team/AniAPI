using Microsoft.Extensions.Hosting;
using SyncService.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService
{
    public class Worker : BackgroundService
    {
        private readonly AnimeScraperWorker _animeScraper;
        private readonly WebsiteScraperWorker _websiteScraper;
        private readonly SongScraperWorker _songScraper;
        private readonly UserSyncWorker _userSync;

        public Worker()
        {
            _animeScraper = WorkerPool.AnimeScraperWorker;
            _websiteScraper = WorkerPool.WebsiteScraperWorker;
            _songScraper = WorkerPool.SongScraperWorker;
            _userSync = WorkerPool.UserSyncWorker;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Task.Run(async () => await _animeScraper.StartAsync(cancellationToken));

                bool canProceed = false;
                while (!canProceed)
                {
                    Thread.Sleep(60 * 1000);
                
                    if (_animeScraper.HasDoneFirstRound)
                    {
                        canProceed = true;
                    }
                }

                Task.Run(async () => await _userSync.StartAsync(cancellationToken));

                Task.Run(async () => await _websiteScraper.StartAsync(cancellationToken));
                Thread.Sleep(60 * 1000);
                
                Task.Run(async () => await _songScraper.StartAsync(cancellationToken));
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(StartAsync(cancellationToken));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
