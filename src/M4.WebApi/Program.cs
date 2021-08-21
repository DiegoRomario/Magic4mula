using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

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
            .ConfigureAppConfiguration((context, config) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
                var settings = config.Build();
                var environment = context.HostingEnvironment.EnvironmentName;
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
            .ConfigureAppConfiguration((context, config) =>
            {
                var buildConfiguration = config.Build();

                var kvURL = buildConfiguration["KeyVaultConfig:KVUrl"];
                var tenantId = buildConfiguration["KeyVaultConfig:TenantId"];
                var clientId = buildConfiguration["KeyVaultConfig:ClientId"];
                var clientSecret = buildConfiguration["KeyVaultConfig:ClientSecretId"];

                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                var client = new SecretClient(new Uri(kvURL), credential);
                config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}