using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EmailSender;
using EmailSender.Core;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EmailSender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<EmailQueue>();
            builder.Services.AddTransient<EmailCreator>();
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
            IConfigurationRoot configuration = configurationBuilder.Build();
            var csAppConfig = configuration["ConnectionStrings:AppConfig"];
            builder.ConfigurationBuilder.AddAzureAppConfiguration(csAppConfig);
            configuration = builder.ConfigurationBuilder.Build();
            var kvURL = configuration["KeyVaultConfig:KVUrl"];
            var tenantId = configuration["KeyVaultConfig:TenantId"];
            var clientId = configuration["KeyVaultConfig:ClientId"];
            var clientSecret = configuration["KeyVaultConfig:ClientSecretId"];
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var client = new SecretClient(new Uri(kvURL), credential);
            builder.ConfigurationBuilder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
        }

    }
}