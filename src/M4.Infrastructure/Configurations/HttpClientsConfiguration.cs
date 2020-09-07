using M4.Infrastructure.Configurations.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace M4.Infrastructure.Configurations
{
    public static class HttpClientsConfiguration
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services )
        {
            services.AddHttpClient("acoes", (sp, client) =>
            {
                string url = sp.GetService<IOptions<HttpClients>>().Value.Acoes;

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Add("User-Agent", "Magic4mula");
            });

            return services;
        }
    }
}