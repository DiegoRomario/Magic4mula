using M4.Infrastructure.Configurations;
using M4.Infrastructure.Services.Email;
using M4.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using M4.Infrastructure.Configurations.Models;
using M4.WebApi.Configurations;

namespace M4.WebApi
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IHostEnvironment hostEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (hostEnvironment.IsDevelopment())
            {
                builder.AddUserSecrets(typeof(Startup).Assembly, true, true);
            }

            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddCustomHealthChecks(_configuration);
            services.AddSwaggerConfiguration();
            services.AddIdentityConfiguration(_configuration);
            services.Configure<Urls>(_configuration.GetSection("Urls"));
            services.Configure<HttpClients>(_configuration.GetSection("HttpClients"));
            services.Configure<EmailConfiguration>(_configuration.GetSection("EmailConfiguration"));
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddAutoMapperProfile();
            services.AddHttpClients();
            services.RegistryServices();
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCustomHealthChecks();
            app.UseSwaggerConfigurations();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }
}
