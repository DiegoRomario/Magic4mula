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
        public string Titulo { get; private set; }
        public string Mensagem { get; private set; }
        public string Destinatarios { get; private set; }
        public DateTime DataEnvio { get; private set; }
        public bool Enviado { get; private set ; }

        public void MarcarEmailComoEnviado()
        {
            Enviado = true;
        }
    }
}
