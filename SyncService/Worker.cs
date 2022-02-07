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
            _animeScraper = new AnimeScraperWorker();
            _websiteScraper = new WebsiteScraperWorker();
            _songScraper = new SongScraperWorker();
            _userSync = new UserSyncWorker();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(ExecuteAsync(cancellationToken));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                Task.Run(async () => await _animeScraper.StartAsync(stoppingToken));
                
                Task.Run(async () => await _userSync.StartAsync(stoppingToken));

                Task.Run(async () => await _websiteScraper.StartAsync(stoppingToken));
                Thread.Sleep(10 * 1000);
                
                Task.Run(async () => await _songScraper.StartAsync(stoppingToken));
            }

            return Task.CompletedTask;
        }
    }
}
