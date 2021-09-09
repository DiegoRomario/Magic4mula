using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EmailSender;
using M4.Domain.Core;
using M4.Domain.Interfaces;
using M4.Infrastructure.Configurations;
using M4.Infrastructure.Configurations.Models;
using M4.Infrastructure.Services.Email;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EmailSender
{
    public class Startup : FunctionsStartup
    {
        public IConfigurationRoot _configuration { get; set; }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddEFConfiguration(_configuration);
            builder.Services.AddTransient<IEmailQueue, EmailQueue>();
            builder.Services.AddTransient<IEmailCreator, EmailCreator>();
            builder.Services.AddOptions<EmailConfiguration>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("EmailConfiguration").Bind(settings);
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            IConfigurationBuilder configurationBuilder = builder.ConfigurationBuilder
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Startup>();
            _configuration = configurationBuilder.Build();
            var csAppConfig = _configuration["ConnectionStrings:AppConfig"];
            builder.ConfigurationBuilder.AddAzureAppConfiguration(csAppConfig);
            _configuration = builder.ConfigurationBuilder.Build();
            var kvURL = _configuration["KeyVaultConfig:KVUrl"];
            var tenantId = _configuration["KeyVaultConfig:TenantId"];
            var clientId = _configuration["KeyVaultConfig:ClientId"];
            var clientSecret = _configuration["KeyVaultConfig:ClientSecretId"];
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var client = new SecretClient(new Uri(kvURL), credential);
            _configuration = builder.ConfigurationBuilder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions()).Build();
        }

    }
}