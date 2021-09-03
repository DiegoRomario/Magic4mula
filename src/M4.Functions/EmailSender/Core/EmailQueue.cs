using M4.Infrastructure.Data.Context;
using M4.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace EmailSender.Core
{
    public class EmailQueue
    {
        private readonly EmailCreator _emailCreator;
        private readonly IConfiguration _configuration;

        public EmailQueue(EmailCreator emailCreator, IConfiguration configuration)
        {
            _emailCreator = emailCreator;
            _configuration = configuration;
        }

        public void DequeueEmail()
        {
            var connectionString = _configuration.GetConnectionString("MagicFormulaSQLServer"); 
            using var dataBase = new MagicFormulaDbContext(connectionString);
            var emailRequests = dataBase.Set<EmailSolicitacao>()
                .Where(s => !s.Enviado).AsNoTracking().ToList();

            foreach (var request in emailRequests)
            {
                try
                {
                    _emailCreator.SendEmail(request.Titulo, request.Mensagem, request.Destinatarios);
                    request.DataEnvio = DateTime.Now;
                    request.Enviado = true;
                    dataBase.EmailSolicitacao.Update(request);
                    dataBase.SaveChanges();
                }
                catch 
                {
                    throw;
                }

            }
        }

    }
}
