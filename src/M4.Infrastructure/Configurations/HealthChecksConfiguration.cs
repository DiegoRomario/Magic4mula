using HealthChecks.UI.Client;
using M4.Infrastructure.Services.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace M4.Infrastructure.Configurations
{
    public static class HealthChecksConfiguration
    {
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddHealthChecks()
            .AddCheck("Usuários Cadastrados", new SqlServerHealthCheck(configuration))
            .AddSqlServer(configuration.GetConnectionString("M4Connection"), name: "Banco de dados SQL Server");
            services.AddHealthChecksUI().AddInMemoryStorage();
            return services;
        }

        public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
        {
            const string healthCheckApiPath = "/health-checks";
            app.UseHealthChecks(healthCheckApiPath,
            new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            })
            .UseHealthChecksUI(options =>
            {
                options.UIPath = $"{healthCheckApiPath}-ui";
                options.ApiPath = $"{healthCheckApiPath}-api";
                options.UseRelativeApiPath = false;
                options.UseRelativeResourcesPath = false;
            });
            return app;
        }
    }
}
