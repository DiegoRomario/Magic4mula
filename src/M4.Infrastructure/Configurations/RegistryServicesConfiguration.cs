using M4.Infrastructure.Services.Http;
using Microsoft.Extensions.DependencyInjection;

namespace M4.Infrastructure.Configurations
{
    public static class RegistryServicesConfiguration
    {
        public static IServiceCollection RegistryServices(this IServiceCollection services)
        {
            services.AddTransient<IAcoesService, AcoesService>();
            return services;
        }
    }
}
