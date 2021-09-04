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
            IConfigurationRoot configuration =  configurationBuilder.Build();
            var csAppConfig = configuration["ConnectionStrings:AppConfig"];
            builder.ConfigurationBuilder.AddAzureAppConfiguration(csAppConfig);
        }

    }
}