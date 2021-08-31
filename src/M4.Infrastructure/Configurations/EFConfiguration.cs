using M4.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace M4.Infrastructure.Configurations
{
    public static class EFConfiguration
    {
        public static IServiceCollection AddEFConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MagicFormulaDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MagicFormulaSQLServer")));
            return services;
        }
    }
}
