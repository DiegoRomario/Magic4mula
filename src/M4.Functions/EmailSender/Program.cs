using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using M4.Domain.Core;
using M4.Domain.Interfaces;
using M4.Infrastructure.Configurations;
using M4.Infrastructure.Configurations.Models;
using M4.Infrastructure.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace EmailSender
{
    public class Program
    {
        static Task Main(string[] args)
        {
            IConfigurationRoot _configuration = null;

            IHost host = new HostBuilder()
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddCommandLine(args);
            })
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
            services.AddLogging();
            services.AddEFConfiguration(_configuration);
            services.AddTransient<IEmailQueue, EmailQueue>();
            services.AddTransient<IEmailCreator, EmailCreator>();
            services.AddOptions<EmailConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("EmailConfiguration").Bind(settings);
            });
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddUserSecrets<Program>();
                IConfigurationRoot settings = config.Build();
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(settings["ConnectionStrings:AppConfig"]);
                });
                _configuration = config.Build();
                var kvURL = _configuration["KeyVaultConfig:KVUrl"];
                var tenantId = _configuration["KeyVaultConfig:TenantId"];
                var clientId = _configuration["KeyVaultConfig:ClientId"];
                var clientSecret = _configuration["KeyVaultConfig:ClientSecretId"];
                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                var client = new SecretClient(new Uri(kvURL), credential);
                _configuration = config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions()).Build();
            })
            .Build();

            return host.RunAsync();
        }
    }
}