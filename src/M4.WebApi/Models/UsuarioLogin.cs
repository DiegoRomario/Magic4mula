﻿using System.ComponentModel.DataAnnotations;

namespace M4.WebApi.Models
{
    public class UsuarioLogin
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} deve ser um e-mail válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(50,ErrorMessage = "O campo {0} deve conter de {2} a {1} caracteres", MinimumLength = 8)]
        public string Senha { get; set; }

    }
}
