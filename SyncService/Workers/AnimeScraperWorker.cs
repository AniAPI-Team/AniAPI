using Microsoft.Extensions.Hosting;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Workers
{
    public class AnimeScraperWorker : BackgroundService
    {
        private IService _service = new AnimeScraperService();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _service.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _service.Work();
                    _service.Wait();
                }
                catch(Exception ex)
                {
                    _service.Stop(ex);
                }
            }
        }
    }
}
