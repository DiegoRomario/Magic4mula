using M4.WebApi.Tests.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;
using M4.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace M4.WebApi.Tests
{
    public class IntegrationTests : IClassFixture<M4Factory<StartupTesting>>
    {
        private const string APP_SETTINGS_TESTING_PATH = "Config/appsettings.Testing.json";
        private const string SOLUTION_RELATIVE_PATH = "src/M4.WebApi";
        private readonly WebApplicationFactory<StartupTesting> _factory;

        public IntegrationTests(M4Factory<StartupTesting> factory)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, APP_SETTINGS_TESTING_PATH);

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(SOLUTION_RELATIVE_PATH);

                builder.ConfigureAppConfiguration(conf =>
                {
                    conf.AddJsonFile(configPath).AddUserSecrets<IntegrationTests>().AddEnvironmentVariables();
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
                });
            });
        }

        [Fact(DisplayName = "Obter status 200 ao buscar 5 ações magic formula (Usuário não logado)")]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObter5MagicFormulaFoiChamado_QuandoOUsuarioEstiverDeslogado_DeveRetornar5PrimeirosRegistros()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("https://localhost/api/acoes/obter-5-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            var registros = JsonSerializer.Deserialize<IEnumerable<AcaoClassificacao>>(body);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5, registros.Count());
        }

        [Fact(DisplayName = "Obter status 401 ao buscar todas ações magic formula (Usuário não logado)")]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObterTodasMagicFormulaFoiChamado_QuandoOUsuarioEstiverDeslogado_DeveRetornarOStatus401()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("https://localhost/api/acoes/obter-todas-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(string.IsNullOrEmpty(body));
        }
    }
}

