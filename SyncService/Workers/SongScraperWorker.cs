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
    public class SongScraperWorker : BackgroundService
    {
        private IService _service = new SongScraperService();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _service.Start(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _service.Work();
                    _service.Wait();
                }
                catch (TaskCanceledException ex)
                {
                    _service.kill(ex);
                }
                catch (Exception ex)
                {
                    _service.Stop(ex);
                }
            }
        }
    }
}
