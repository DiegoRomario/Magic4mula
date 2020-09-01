using System.Collections.Generic;

namespace M4.WebApi.Models
{
    public class UsuarioRespostaLogin 
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public IEnumerable<UsuarioClaim> Claims { get; set; }
    }
}
