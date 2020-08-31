using System.Threading.Tasks;
using System.Web;
using M4.Infrastructure.Email;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

        public UsuarioController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender, IOptions<Urls> URLs)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _uRLs = URLs.Value;
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

            if (result.Succeeded) return Ok();

            return BadRequest();

        }

        [HttpPost("solicitar-nova-senha")]
        public async Task<ActionResult> SolicitarNovaSenha(UsuarioSolicitacaoSenha usuarioSolicitacaoSenha)
        {
            if (!ModelState.IsValid) BadRequest();
            var usuario = await _userManager.FindByEmailAsync(usuarioSolicitacaoSenha.Email);
            if (usuario == null) NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            token = HttpUtility.UrlEncode(token);
            string urlConfirmacao = string.Format(_uRLs.CadastroNovaSenha, usuario.Email, token);
            var message = new EmailMessage(new string[] { usuario.Email }, "Redefinição de senha", urlConfirmacao, null);
            await _emailSender.SendEmailAsync(message);

            return Ok();
        }

        [HttpPost("redefinir-senha")]
        public async Task<ActionResult> RedefinirSenha(UsuarioAlteracaoSenha usuarioAlteracaoSenha)
        {
            if (!ModelState.IsValid) return BadRequest();

            var usuario = await _userManager.FindByEmailAsync(usuarioAlteracaoSenha.Email);
            if (usuario == null) NotFound();

            var token = HttpUtility.UrlDecode(usuarioAlteracaoSenha.Token);
            var response = await _userManager.ResetPasswordAsync(usuario, token, usuarioAlteracaoSenha.Senha);
            if (!response.Succeeded) StatusCode(500, response.Errors);

            return Ok();

        }

    }


}
