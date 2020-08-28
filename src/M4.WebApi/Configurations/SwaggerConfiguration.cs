using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M4.WebApi.Configurations
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Magic4mula API - 🧙",
                    Description = "Filtrando ações de forma simples e eficiente, inspirado pelas melhores literaturas de investimentos.",
                    Contact = new OpenApiContact() { Name = "Diego Romário", Email = "diego.romario@outlook.com" },
                    License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }

                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfigurations(this IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "documentacao/{documentName}/documentacao.json";
            });
            app.UseSwaggerUI(
                options =>
                {
                    options.RoutePrefix = "documentacao";
                    options.SwaggerEndpoint($"/documentacao/v1/documentacao.json", "v1");
                });

            return app;
        }
    }
}
