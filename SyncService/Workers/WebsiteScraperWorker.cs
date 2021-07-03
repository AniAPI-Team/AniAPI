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
    public class WebsiteScraperWorker : BackgroundService
    {
        private IService _service = new WebsiteScraperService();

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
