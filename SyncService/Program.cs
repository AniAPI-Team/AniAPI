using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SyncService
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IService> services = new List<SyncService.IService>()
            {
                new AnimeScraperService(),
                new WebsiteScraperService(),
                new SongScraperService(),
            };

            new Thread(services[0].Start).Start();

            while(services[0].ServiceStatus.Status != Commons.Enums.ServiceStatusEnum.WAITING)
            {
                Thread.Sleep(60 * 1000);
            }
            
            for(int i = 1; i < services.Count; i++)
            {
                new Thread(services[i].Start).Start();
                Thread.Sleep(60 * 1000);
            }

            Console.ReadLine();
        }
    }
}
