using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace M4.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                if (hostingContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
                var settings = config.Build();
                var environment = hostingContext.HostingEnvironment.EnvironmentName;
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(settings["ConnectionStrings:AppConfig"])
                        .ConfigureRefresh(refresh =>
                        {
                            refresh.Register("Urls:ConfirmacaoEmail", true);
                        }).UseFeatureFlags(opt =>
                        {
                            opt.Label = environment;
                            opt.CacheExpirationInterval = TimeSpan.FromSeconds(5);
                        });
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
