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
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Microsoft.FeatureManagement;

namespace M4.WebApi
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAzureAppConfiguration();
            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", build =>
            {
                build.WithOrigins("*")
                     .AllowAnyMethod()
                     .AllowAnyHeader();
            }));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddFeatureManagement();
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
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter()); options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAzureAppConfiguration();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("ApiCorsPolicy");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCustomHealthChecks();
            app.UseSwaggerConfigurations();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
