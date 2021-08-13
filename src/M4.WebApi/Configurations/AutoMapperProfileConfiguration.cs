using M4.WebApi.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace M4.WebApi.Configurations
{
    public static class AutoMapperProfileConfiguration
    {
        public static IServiceCollection AddAutoMapperProfile(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DomainToViewModelProfile));
            return services;
        }
    }
}
