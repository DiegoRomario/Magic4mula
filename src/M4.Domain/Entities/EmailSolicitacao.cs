using System;

namespace M4.Domain.Entities
{ 
    public class EmailSolicitacao : Entity
    {
        public EmailSolicitacao() { }
        public EmailSolicitacao(string titulo, string mensagem, string destinatarios)
        {
            Titulo = titulo;
            Mensagem = mensagem;
            Destinatarios = destinatarios;
            DataEnvio = DateTime.Now;
        }
        public string Titulo { get; init; }
        public string Mensagem { get; init; }
        public string Destinatarios { get; init; }
        public DateTime DataEnvio { get; init; }
        public bool Enviado { get; init ; }
    }
}
