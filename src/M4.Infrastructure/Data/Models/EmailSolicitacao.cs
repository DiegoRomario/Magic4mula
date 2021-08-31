using System;

namespace M4.Infrastructure.Data.Models
{

    public class EmailSolicitacao
    {
        public EmailSolicitacao() { }
        public EmailSolicitacao(int id, string titulo, string mensagem, string destinatarios, DateTime dataEnvio, bool enviado)
        {
            Id = id;
            Titulo = titulo;
            Mensagem = mensagem;
            Destinatarios = destinatarios;
            Enviado = enviado;
            DataEnvio = dataEnvio;
        }

        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public string Destinatarios { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Enviado { get; set; }
    }
}
