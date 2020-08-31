using System.ComponentModel.DataAnnotations;
namespace M4.WebApi.Models
{
    public class UsuarioSolicitacaoSenha
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} deve ser um e-mail válido")]
        public string Email { get; set; }
    }
}
