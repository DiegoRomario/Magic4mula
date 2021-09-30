using System;

namespace M4.Domain.Entities
{ 
    public class EmailSolicitacao : Entity
    {
        public EmailSolicitacao() { }
        public EmailSolicitacao(string titulo, string mensagem, string nomeDestinatario, string emailDestinatario)
        {
            Titulo = titulo;
            Mensagem = mensagem;
            NomeDestinatario = nomeDestinatario;
            EmailDestinatario = emailDestinatario;
            DataEnvio = DateTime.Now;
        }
        public string Titulo { get; init; }
        public string Mensagem { get; init; }
        public string NomeDestinatario { get; init; }
        public string EmailDestinatario { get; init; }
        public DateTime DataEnvio { get; init; }
        public bool Enviado { get; init ; }
    }
}
