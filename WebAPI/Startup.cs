using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Middlewares;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Cors
            services.AddCors(o => o.AddPolicy("MyCorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddMvc(o =>
            {
                o.UseGeneralRoutePrefix("api/v{version:apiVersion}");
            }).AddJsonOptions(options => { 
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            IConfiguration config = new ConfigurationBuilder().
                SetBasePath(Directory.GetCurrentDirectory()).
                AddInMemoryCollection(new AppSettingsCollection().GetConfiguration()).
                Build();

            services.Configure<AppSettings>(config);

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
                c.DocumentFilter<EnumDocumentFilter>();

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebAPI.xml");
                c.IncludeXmlComments(filePath);
            });

            services.AddHttpContextAccessor();

            services.AddSingleton<IRateLimitDependency, RateLimitDependency>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }

            // Enable Cors
            app.UseCors("MyCorsPolicy");

            app.UseRateLimitMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            loggerFactory.AddFile("Logs/log-{Date}.txt");
        }
    }
}
