﻿using Bogus;
using M4.Infrastructure.Data.Context;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;


namespace M4.WebApi.Tests.Config
{

    [CollectionDefinition(nameof(IntegrationApiTestsFixtureCollection))]
    public class IntegrationApiTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<StartupTesting>> { }

    public class IntegrationTestsFixture<TStartup> : IDisposable, IClassFixture<M4Factory<TStartup>> where TStartup : class
    {
        private const string APP_SETTINGS_TESTING_PATH = "Config/appsettings.Testing.json";
        private const string SOLUTION_RELATIVE_PATH = "src/M4.WebApi";
        public UsuarioCadastro UsuarioCadastro;
        public UsuarioRespostaLogin UsuarioLogado;

        private readonly WebApplicationFactory<TStartup> _WebApplicationFactory;
        public HttpClient Client;
        private UserIdentityDbContext _Context;

        public IntegrationTestsFixture()
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, APP_SETTINGS_TESTING_PATH);
            var optionsBuilder = new DbContextOptionsBuilder<UserIdentityDbContext>();

            _WebApplicationFactory = new M4Factory<TStartup>().WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot(SOLUTION_RELATIVE_PATH);

                builder.ConfigureAppConfiguration(conf =>
                {
                    conf.AddJsonFile(configPath)
                        .AddUserSecrets<IntegrationTests>()
                        .AddEnvironmentVariables();

                    var connectionString = conf.Build()["ConnectionStrings:MagicFormulaSQLServer"];
                    optionsBuilder.UseSqlServer(connectionString);
                    this._Context = new UserIdentityDbContext(optionsBuilder.Options);
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
            Client = _WebApplicationFactory.CreateClient(clientOptions);
            GerarUsuario();
        }

        public void GerarUsuario()
        {
            var faker = new Faker("pt_BR");
            var senha = faker.Internet.Password(length: 8, prefix: "D3mº");
            var nome = faker.Name.FirstName();
            var sobrenome = faker.Name.LastName();
            UsuarioCadastro = new UsuarioCadastro { Email = faker.Internet.Email(nome,sobrenome), Senha = senha, SenhaConfirmacao = senha, Nome = nome, Sobrenome = sobrenome };
        }

        public async Task LogarUsuario()
        {
            var usuarioLogin = new UsuarioLogin
            {
                Email = UsuarioCadastro.Email,
                Senha = UsuarioCadastro.Senha
            };

            var response = await Client.PostAsJsonAsync("usuario/entrar", usuarioLogin);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            UsuarioLogado = JsonSerializer.Deserialize<UsuarioRespostaLogin>(body, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        }
        public async Task<HttpResponseMessage> CadastrarUsuario()
        {
            _Context.Database.EnsureDeleted();
            _Context.Database.EnsureCreated();
            return await Client.PostAsJsonAsync("usuario/cadastrar", UsuarioCadastro);
        }

        public void Dispose()
        {
            Client.Dispose();
            _WebApplicationFactory.Dispose();
        }
    }
}