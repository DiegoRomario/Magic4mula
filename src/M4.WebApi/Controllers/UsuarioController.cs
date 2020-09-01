using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using M4.Infrastructure.Configurations.Models;
using M4.Infrastructure.Email;
using M4.Infrastructure.Models;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace M4.WebApi.Controllers
{
    [Route("api/usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly Urls _uRLs;
        private readonly AppSettings _appSettings;

        public UsuarioController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
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
            if (!ModelState.IsValid) return BadRequest();

            IdentityUser usuario = new IdentityUser
            {
                UserName = usuarioCadastro.Email,
                Email = usuarioCadastro.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(usuario, usuarioCadastro.Senha);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(usuario, true);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
                token = HttpUtility.UrlEncode(token);
                string urlConfirmacao = string.Format(_uRLs.ConfirmacaoEmail, usuario.Email, token);
                var message = new EmailMessage(new string[] { usuario.Email }, "Confirmação de e-mail", urlConfirmacao, null);
                await _emailSender.SendEmailAsync(message);
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet("confirmar-email")]
        public async Task<ActionResult> ConfirmarEmail([FromQuery] string email, [FromQuery] string token)
        {
            IdentityUser usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null) return NotFound();

            IdentityResult respose = await _userManager.ConfirmEmailAsync(usuario, token);
            if (respose.Succeeded)
                return Ok();

            return StatusCode(500, respose.Errors);

        }

        [HttpPost("entrar")]
        public async Task<ActionResult> Entrar(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, true, true);

            UsuarioRespostaLogin usuario = await GerarJwtToken(usuarioLogin.Email);
            if (result.Succeeded) return Ok(usuario);

            return BadRequest();

        }

        [HttpPost("solicitar-nova-senha")]
        public async Task<ActionResult> SolicitarNovaSenha(UsuarioSolicitacaoSenha usuarioSolicitacaoSenha)
        {
            if (!ModelState.IsValid) BadRequest();
            IdentityUser usuario = await _userManager.FindByEmailAsync(usuarioSolicitacaoSenha.Email);
            if (usuario == null) NotFound();

            string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            token = HttpUtility.UrlEncode(token);
            string urlConfirmacao = string.Format(_uRLs.CadastroNovaSenha, usuario.Email, token);
            EmailMessage message = new EmailMessage(new string[] { usuario.Email }, "Redefinição de senha", urlConfirmacao, null);
            await _emailSender.SendEmailAsync(message);

            return Ok();
        }

        [HttpPost("redefinir-senha")]
        public async Task<ActionResult> RedefinirSenha(UsuarioAlteracaoSenha usuarioAlteracaoSenha)
        {
            if (!ModelState.IsValid) return BadRequest();

            IdentityUser usuario = await _userManager.FindByEmailAsync(usuarioAlteracaoSenha.Email);
            if (usuario == null) NotFound();

            string token = HttpUtility.UrlDecode(usuarioAlteracaoSenha.Token);
            IdentityResult response = await _userManager.ResetPasswordAsync(usuario, token, usuarioAlteracaoSenha.Senha);
            if (!response.Succeeded) StatusCode(500, response.Errors);

            return Ok();
        }

        private async Task<UsuarioRespostaLogin> GerarJwtToken(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(usuario);

            var identityClaims = await ObterClaimsUsuario(claims, usuario);
            var encodedToken = CodificarToken(identityClaims);

            return ObterRespostaToken(encodedToken, usuario, claims);
        }

        private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, IdentityUser user)
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

        private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            return new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationTimeInHours).TotalSeconds,
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
            };
        }
    }
}
