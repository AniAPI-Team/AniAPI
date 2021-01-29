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
                //new WebsiteScraperService()
            };

            foreach(IService service in services)
            {
                new Thread(service.Start).Start();
            }

            Console.ReadLine();
        }
    }
}
