using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            string hostName = builder.Configuration["HostName"];
            string protocol = builder.Configuration["Protocol"];
            int port = int.Parse(builder.Configuration["Port"]);
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri($"{protocol}://{hostName}:{port}/") });

            builder.Services.AddScoped<Generic>();

            bool useCustomVideoPlayer = bool.Parse(builder.Configuration["UseCustomVideoPlayer"]);
            bool useDarkTheme = bool.Parse(builder.Configuration["UseDarkTheme"]);
            builder.Services.AddSingleton<UIConfiguration>(_ => new UIConfiguration(useCustomVideoPlayer, useDarkTheme));

            await builder.Build().RunAsync();
        }
    }
}
