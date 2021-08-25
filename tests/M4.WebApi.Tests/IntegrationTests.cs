using M4.WebApi.Models;
using M4.WebApi.Tests.Config;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace M4.WebApi.Tests
{
    [Collection(nameof(IntegrationApiTestsFixtureCollection))]
    public class IntegrationTests
    {
        private readonly IntegrationTestsFixture<StartupTesting> _testsFixture;

        public IntegrationTests(IntegrationTestsFixture<StartupTesting> testsFixture)
        {
            _testsFixture = testsFixture;
        }

        [Fact(DisplayName = "Obter status 200 ao buscar 5 ações magic formula (Usuário não logado)"), TestPriority(1)]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObter5MagicFormulaFoiChamado_QuandoOUsuarioEstiverDeslogado_DeveRetornar5PrimeirosRegistros()
        {
            // Arrange
            // Act
            var response = await _testsFixture.Client.GetAsync("acoes/obter-5-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            var registros = JsonSerializer.Deserialize<IEnumerable<AcaoClassificacao>>(body);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5, registros.Count());
        }

        [Fact(DisplayName = "Obter status 401 ao buscar todas ações magic formula (Usuário não logado)"), TestPriority(2)]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObterTodasMagicFormulaFoiChamado_QuandoOUsuarioEstiverDeslogado_DeveRetornarOStatus401()
        {
            // Arrange
            // Act
            var response = await _testsFixture.Client.GetAsync("acoes/obter-todas-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(string.IsNullOrEmpty(body));
        }

        [Fact(DisplayName = "Obter status 204 ao tentar cadastrar usuário com dados válidos"), TestPriority(3)]
        [Trait("Integração", "Usuario")]
        public async Task DadosQueOCadastroDeUsuariosFoiChamado_QuandoConterDadosValidos_DeveCadastrarUsuarioEEnviarEmailDeConfirmacao()
        {
            // Arrange
            // Act
            HttpResponseMessage response = await _testsFixture.CadastrarUsuario();
            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }


        [Fact(DisplayName = "Obter dados de usuário cadastrados (token) ao se logar com dados válidos"), TestPriority(4)]
        [Trait("Integração", "Usuario")]
        public async Task DadosQueExisteOUsuarioCadastrado_QuandoOLoginForRequisitado_DeveRetornarOsDadosDoMesmo()
        {
            // Arrange
            // Act
            await _testsFixture.LogarUsuario();
            // Assert
            Assert.NotNull(_testsFixture.UsuarioLogado.AccessToken);
            Assert.True(_testsFixture.UsuarioLogado.AccessToken.Length > 20);
        }

        [Fact(DisplayName = "Obter status 200 ao buscar todas ações magic formula (Usuário logado)"), TestPriority(5)]
        [Trait("Integração", "Ações")]
        public async Task DadoQueObterTodasMagicFormulaFoiChamado_QuandoOUsuarioEstiverLogado_DeveRetornarOStatus200EOsRegistros()
        {
            // Arrange
            // Act
            _testsFixture.Client.AtribuirToken(_testsFixture.UsuarioLogado.AccessToken);
            var response = await _testsFixture.Client.GetAsync("acoes/obter-todas-magic-formula");
            var body = await response.Content.ReadAsStringAsync();
            var acoes = JsonSerializer.Deserialize<IEnumerable<AcaoClassificacao>>(body);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(acoes.Count() > 5);
        }
    }
}

