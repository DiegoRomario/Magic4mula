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
using Microsoft.FeatureManagement;
using M4.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using M4.Domain.Core;
using System.Text.Json.Serialization;

namespace M4.WebApi
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
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
            services.AddEFConfiguration(_configuration);
            services.AddTransient<IEmailQueue, EmailQueue>();
            services.AddTransient<IEmailCreator, EmailCreator>();
            services.Configure<Urls>(_configuration.GetSection("Urls"));
            services.Configure<HttpClients>(_configuration.GetSection("HttpClients"));
            services.Configure<EmailConfiguration>(_configuration.GetSection("EmailConfiguration"));
            services.AddScoped<IEmailQueue, EmailQueue>();
            services.AddAutoMapperProfile();
            services.AddHttpClients();
            services.RegistryServices();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddApplicationInsightsTelemetry(_configuration.GetConnectionString("ApplicationInsightsMagicFormulaWebApi"));
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsEnvironment("Testing"))            
                app.UseAzureAppConfiguration();
            
            if (env.IsDevelopment())           
                app.UseDeveloperExceptionPage();
          
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
