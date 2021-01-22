using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AniApi.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //Get data from Configuration file
            string hostname = builder.Configuration["HostName"];
            string protocol = builder.Configuration["Protocol"];
            int port = int.Parse(builder.Configuration["Port"]);
            bool UseCustomVideoPlayer = bool.Parse(builder.Configuration["UseCustomVideoPlayer"]);

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); //Modificare se si vuole chiamare WebAPI
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(string.Format("{0}://{1}:{2}/", protocol, hostname, port)) });

            builder.Services.AddScoped<Generic>(); //Metodi Custom comuni a tutte le pagine

            builder.Services.AddSingleton<UIConfigurations>(_ => new UIConfigurations(UseCustomVideoPlayer));

            await builder.Build().RunAsync();
        }
    }
}
