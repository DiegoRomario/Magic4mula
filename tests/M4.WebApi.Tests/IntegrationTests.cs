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
using System.Net.Http.Json;
using M4.Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System;

namespace M4.WebApi.Tests
{
    public class IntegrationTests : IClassFixture<M4Factory<StartupTesting>>
    {
        private const string APP_SETTINGS_TESTING_PATH = "Config/appsettings.Testing.json";
        private const string SOLUTION_RELATIVE_PATH = "src/M4.WebApi";
        private readonly WebApplicationFactory<StartupTesting> _factory;
        private UserIdentityDbContext _context { get; set; }
        private HttpClient _client;

        public IntegrationTests(M4Factory<StartupTesting> factory)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, APP_SETTINGS_TESTING_PATH);
            var optionsBuilder = new DbContextOptionsBuilder<UserIdentityDbContext>();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(SOLUTION_RELATIVE_PATH);

                builder.ConfigureAppConfiguration(conf =>
                {
                    conf.AddJsonFile(configPath)
                        .AddUserSecrets<IntegrationTests>()
                        .AddEnvironmentVariables();

                    var connectionString = conf.Build()["ConnectionStrings:MagicFormulaSQLServer"];
                    optionsBuilder.UseSqlServer(connectionString);
                    this._context = new UserIdentityDbContext(optionsBuilder.Options);
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
                });
            });

            var clientOptions = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                BaseAddress = new Uri("http://localhost/api/"),
                HandleCookies = true,
                MaxAutomaticRedirections = 7
            };

            _client = _factory.CreateClient(clientOptions);
        }

        [Fact(DisplayName = "Obter status 200 ao buscar 5 ações magic formula (Usuário não logado)")]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObter5MagicFormulaFoiChamado_QuandoOUsuarioEstiverDeslogado_DeveRetornar5PrimeirosRegistros()
        {
            // Arrange
            // Act
            var response = await _client.GetAsync("acoes/obter-5-magic-formula");
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
            // Act
            var response = await _client.GetAsync("acoes/obter-todas-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(string.IsNullOrEmpty(body));
        }

        [Fact(DisplayName = "Obter status 204 ao tentar cadastrar usuário com dados válidos")]
        [Trait("Integração", "Usuario")]
        public async Task DadosQueOCadastroDeUsuariosFoiChamado_QuandoConterDadosValidos_DeveCadastrarUsuarioEEnviarEmailDeConfirmacao()
        {
            // Arrange
            // Act
            HttpResponseMessage response = await CadastrarUsuario();
            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task<HttpResponseMessage> CadastrarUsuario()
        {
            var usuario = new UsuarioCadastro { Email = "demo@demo.com.br", Senha = "Demo@666", SenhaConfirmacao = "Demo@666", Nome = "Diego", Sobrenome = "Romário" };
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            var response = await _client.PostAsJsonAsync("https://localhost/api/usuario/cadastrar", usuario);
            return response;
        }

        private async Task<UsuarioRespostaLogin> LogarUsuario()
        {
            var usuarioLogin = new UsuarioLogin
            {
                Email = "demo@demo.com.br",
                Senha = "Demo@666"
            };

            var response = await _client.PostAsJsonAsync("usuario/entrar", usuarioLogin);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var usuarioResponse = JsonSerializer.Deserialize<UsuarioRespostaLogin>(body, new JsonSerializerOptions() { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
            return usuarioResponse;
        }

        [Fact(DisplayName = "Obter dados de usuário cadastrados (token) ao se logar com dados válidos")]
        [Trait("Integração", "Usuario")]
        public async Task DadosQueExisteOUsuarioCadastrado_QuandoOLoginForRequisitado_DeveRetornarOsDadosDoMesmo()
        {
            // Arrange
            await CadastrarUsuario();
            // Act
            var response = await LogarUsuario();
            // Assert
            Assert.NotNull(response.AccessToken);
            Assert.True(response.AccessToken.Length > 20);
        }
    }
}

