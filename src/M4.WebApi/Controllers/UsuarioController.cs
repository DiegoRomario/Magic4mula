using M4.Domain.Entities;
using M4.Domain.Interfaces;
using M4.Infrastructure.Configurations.Models;
using M4.Infrastructure.Data.Models;
using M4.Infrastructure.Models;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace M4.WebApi.Controllers
{
    [Route("api/usuario")]
    public class UsuarioController : BaseController
    {
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IEmailQueue _emailQueue;
        private readonly Urls _uRLs;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly ILogger<UsuarioController> _logger;
        private readonly bool isTesting;

        public UsuarioController(SignInManager<UserIdentity> signInManager,
            UserManager<UserIdentity> userManager,
            IEmailQueue emailSender, IOptions<Urls> URLs, IOptions<AppSettings> appSettings, IWebHostEnvironment iHostingEnvironment, ILogger<UsuarioController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailQueue = emailSender;
            _uRLs = URLs.Value;
            _appSettings = appSettings.Value;
            _iHostingEnvironment = iHostingEnvironment;
            isTesting = _iHostingEnvironment.EnvironmentName == "Testing";
            _logger = logger;
        }
        [HttpPost("cadastrar")]
        public async Task<ActionResult> Cadastrar(UsuarioCadastro usuarioCadastro)
        {
            if (!ModelState.IsValid) return BaseResponse(ModelState);

            UserIdentity usuario = new UserIdentity
            {
                UserName = usuarioCadastro.Email,
                Email = usuarioCadastro.Email,
                EmailConfirmed = isTesting,
                Name = usuarioCadastro.Nome,
                LastName = usuarioCadastro.Sobrenome
            };

            IdentityResult result = await _userManager.CreateAsync(usuario, usuarioCadastro.Senha);

            if (result.Succeeded && !isTesting)
            {
                try
                {
                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
                    token = HttpUtility.UrlEncode(token);
                    string encodedEmail = EncodeEmail(usuario.Email);
                    string urlConfirmacao = string.Format(_uRLs.ConfirmacaoEmail, encodedEmail, token);
                    EmailSolicitacao message = new("Confirmação de e-mail", urlConfirmacao, usuario.Name, usuario.Email);
                    await _emailQueue.EnqueueEmailAsync(message);
                    return BaseResponse("Usuário cadastrado com sucesso! Um e-mail será enviado para confirmação do cadastro.");
                }
                catch (Exception ex)
                {
                    await _userManager.DeleteAsync(usuario);
                    string mensagemDeErro = $"Ocorreu um erro durante o cadastro de usuário, por favor entre em contato com o suporte. - {ex.Message}";
                    AdicionarErro(mensagemDeErro);
                    _logger.LogError(ex, mensagemDeErro);
                    return BaseResponse(statusCodeErro: HttpStatusCode.InternalServerError);
                }
            }

            foreach (var error in result.Errors)
                AdicionarErro(error.Description);

            return BaseResponse();
        }



        [HttpPost("confirmar-email")]
        public async Task<ActionResult> ConfirmarEmail(UsuarioConfirmacaoEmail confirmacao)
        {
            string decodedEmail = DecodeEmail(confirmacao.Email);
            UserIdentity usuario = await _userManager.FindByEmailAsync(decodedEmail);
            if (usuario == null) return BaseResponse("Usuário não encontrado", statusCodeErro: HttpStatusCode.NotFound);

            var result = await _userManager.ConfirmEmailAsync(usuario, confirmacao.Token);
            if (result.Succeeded)
                return BaseResponse("E-mail confirmado com sucesso.");

            foreach (var error in result.Errors)
                AdicionarErro(error.Description);

            return BaseResponse();
        }



        [HttpPost("entrar")]
        public async Task<ActionResult> Entrar(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return BaseResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, true, true);

            if (result.Succeeded)
            {
                UsuarioRespostaLogin usuario = await GerarJwtToken(usuarioLogin.Email);
                return BaseResponse(usuario);
            }

            if (result.IsLockedOut) AdicionarErro("Usuário bloqueado por tentativas inválidas. Tente novamente mais tarde.");
            else AdicionarErro("Usuário e/ou Senha incorretos");

            return BaseResponse();

        }


        [HttpPost("solicitar-nova-senha")]
        public async Task<ActionResult> SolicitarNovaSenha(UsuarioSolicitacaoSenha usuarioSolicitacaoSenha)
        {
            if (!ModelState.IsValid) return BaseResponse(ModelState);
            UserIdentity usuario = await _userManager.FindByEmailAsync(usuarioSolicitacaoSenha.Email);
            if (usuario == null) return BaseResponse("Usuário não encontrado", statusCodeErro: HttpStatusCode.NotFound);

            try
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                token = HttpUtility.UrlEncode(token);
                string urlConfirmacao = string.Format(_uRLs.CadastroNovaSenha, usuario.Email, token);
                EmailSolicitacao message = new ("Redefinição de senha", urlConfirmacao, usuario.Name, usuario.Email);
                await _emailQueue.EnqueueEmailAsync(message);
                return BaseResponse("Um e-mail com link para redefinição de senha será enviado.");
            }
            catch (Exception ex)
            {
                string mensagemDeErro = $"Ocorreu um erro durante o cadastro de usuário, por favor entre em contato com o suporte. - {ex.Message}";
                AdicionarErro(mensagemDeErro);
                _logger.LogError(ex, mensagemDeErro);
                return BaseResponse(statusCodeErro: HttpStatusCode.InternalServerError);
            }

        }

        [HttpPost("redefinir-senha")]
        public async Task<ActionResult> RedefinirSenha(UsuarioAlteracaoSenha usuarioAlteracaoSenha)
        {
            if (!ModelState.IsValid) return BaseResponse(ModelState);

            UserIdentity usuario = await _userManager.FindByEmailAsync(usuarioAlteracaoSenha.Email);
            if (usuario == null) return BaseResponse("Usuário não encontrado", statusCodeErro: HttpStatusCode.NotFound);

            string token = HttpUtility.UrlDecode(usuarioAlteracaoSenha.Token);
            var result = await _userManager.ResetPasswordAsync(usuario, token, usuarioAlteracaoSenha.Senha);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    AdicionarErro(error.Description);

                return BaseResponse(result.Errors);
            }

            return BaseResponse("Senha redefinida com sucesso.");
        }

        private async Task<UsuarioRespostaLogin> GerarJwtToken(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(usuario);

            var identityClaims = await ObterClaimsUsuario(claims, usuario);
            var encodedToken = CodificarToken(identityClaims);

            return ObterRespostaToken(encodedToken, usuario, claims);
        }

        private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, UserIdentity user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            foreach (var userRole in userRoles)
                claims.Add(new Claim("role", userRole));

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private string CodificarToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.Audience,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationTimeInHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, UserIdentity user, IEnumerable<Claim> claims)
        {
            return new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationTimeInHours).TotalSeconds,
                Id = user.Id,
                Email = user.Email,
                Nome = user.Name,
                Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
            };
        }
        private static string EncodeEmail(string email)
        {
            byte[] textoAsBytes = Encoding.ASCII.GetBytes(email);
            string base64Email = Convert.ToBase64String(textoAsBytes);
            string encodedEmail = HttpUtility.UrlEncode(base64Email);
            return encodedEmail;
        }
        private static string DecodeEmail(string email)
        {
            byte[] dadosAsBytes = Convert.FromBase64String(email);
            string base64Email = Encoding.ASCII.GetString(dadosAsBytes);
            string decodedEmail = HttpUtility.UrlDecode(base64Email);
            return decodedEmail;
        }
    }
}
