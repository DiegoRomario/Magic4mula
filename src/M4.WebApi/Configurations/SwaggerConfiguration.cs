using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;

namespace M4.WebApi.Configurations
{
    public static class SwaggerConfiguration
    {
        private const string BASE_DOC_URL = "documentacao";

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {

            services.AddSwaggerGen(s =>
            {
                s.DocumentFilter<FeatureGateDocumentFilter>();
                s.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Magic4mula API - 🧙",
                    Version = "1.0",
                    Description = "Filtrando ações de forma simples e eficiente, inspirado pelas melhores literaturas de investimentos.",
                    Contact = new OpenApiContact() { Name = "Diego Romário", Email = "diego.romario@outlook.com" },
                    License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") },
                    TermsOfService = new Uri("https://opensource.org/licenses/MIT"),

                });
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "O token JWT deve ser informado da seguinte maneira: Bearer {seu token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            }
                        }, new string[] { }
                    }
                });

            });
            return services;
        }

        public static IApplicationBuilder UseSwaggerConfigurations(this IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = BASE_DOC_URL + "/{documentName}/" + BASE_DOC_URL + ".json";
            });
            app.UseSwaggerUI(
                options =>
                {
                    options.RoutePrefix = BASE_DOC_URL;
                    options.SwaggerEndpoint($"/{BASE_DOC_URL}/v1/{BASE_DOC_URL}.json", "v1");
                });

            RewriteOptions option = new();
            option.AddRedirect("^$", BASE_DOC_URL);
            app.UseRewriter(option);

            return app;
        }
    }
}
