using M4.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace M4.Infrastructure.Configurations
{
    public static class IdentityConfiguration
    {
        public static IServiceCollection AddIdentityConfiguration (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MyIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("M4Connection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MyIdentityDbContext>()
                .AddDefaultTokenProviders();
            
            return services;
        }
    }
}
