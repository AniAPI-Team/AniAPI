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

        public Worker()
        {
            _animeScraper = WorkerPool.AnimeScraperWorker;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await _animeScraper.StartAsync(cancellationToken));

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
