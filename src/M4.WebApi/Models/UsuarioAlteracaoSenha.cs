using System.ComponentModel.DataAnnotations;

namespace M4.WebApi.Models
{
    public class UsuarioAlteracaoSenha
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(50, ErrorMessage = "O campo {0} deve conter de {2} a {1} caracteres", MinimumLength = 8)]
        public string Senha { get; set; }
        [Compare("Senha", ErrorMessage = "A confirmação não confere com a senha informada")]
        public string SenhaConfirmacao { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
