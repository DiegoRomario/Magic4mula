using System.Threading.Tasks;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace M4.WebApi.Controllers
{
    [Route("api/usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UsuarioController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost("entrar")]
        public async Task<ActionResult> Entrar(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, true, true);

            if (result.Succeeded) return Ok();

            return BadRequest();

        }
    }
}
