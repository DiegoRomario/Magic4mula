using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using M4.Infrastructure.Configurations.Models;
using M4.Infrastructure.Services.Email;
using M4.Infrastructure.Models;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using M4.Infrastructure.Data.Identity;

namespace M4.WebApi.Controllers
{
    [Route("api/usuario")]
    public class UsuarioController : BaseController
    {
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly Urls _uRLs;
        private readonly AppSettings _appSettings;

        public UsuarioController(SignInManager<UserIdentity> signInManager,
            UserManager<UserIdentity> userManager,
            IEmailSender emailSender, IOptions<Urls> URLs, IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _uRLs = URLs.Value;
            _appSettings = appSettings.Value;
        }
        [HttpPost("cadastrar")]
        public async Task<ActionResult> Cadastrar(UsuarioCadastro usuarioCadastro)
        {
            if (!ModelState.IsValid) return BaseResponse(ModelState);

            UserIdentity usuario = new UserIdentity
            {
                UserName = usuarioCadastro.Email,
                Email = usuarioCadastro.Email,
                EmailConfirmed = false,
                Name = usuarioCadastro.Nome,
                LastName = usuarioCadastro.Sobrenome
            };

            IdentityResult result = await _userManager.CreateAsync(usuario, usuarioCadastro.Senha);

            if (result.Succeeded)
            {
                try
                {
                    string token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
                    token = HttpUtility.UrlEncode(token);
                    string urlConfirmacao = string.Format(_uRLs.ConfirmacaoEmail, usuario.Email, token);
                    EmailMessage message = new EmailMessage(new string[] { usuario.Email }, "Confirmação de e-mail", urlConfirmacao, null);
                    await _emailSender.SendEmailAsync(message);
                    return BaseResponse("Usuário cadastrado com sucesso! Um e-mail foi enviado para confirmação do cadastro.");
                }
                catch (Exception ex)
                {
                    await _userManager.DeleteAsync(usuario);
                    AdicionarErro($"Ocorreu um erro durante o cadastro de usuário, por favor entre em contato com o suporte. {ex.Message} || {ex.StackTrace}");
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
            UserIdentity usuario = await _userManager.FindByEmailAsync(confirmacao.Email);
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
                EmailMessage message = new EmailMessage(new string[] { usuario.Email }, "Redefinição de senha", urlConfirmacao, null);
                await _emailSender.SendEmailAsync(message);
                return BaseResponse("E-mail com link para redefinição de senha enviado com sucesso.");
            }
            catch (Exception ex)
            {
                AdicionarErro($"Ocorreu um erro durante o cadastro de usuário, por favor entre em contato com o suporte. {ex.Message} || {ex.StackTrace}");
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
    }
}
