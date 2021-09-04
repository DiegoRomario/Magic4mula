using M4.Infrastructure.Data.Context;
using M4.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EmailSender.Core
{
    public class EmailQueue
    {
        private readonly EmailCreator _emailCreator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailQueue> _logger;

        public EmailQueue(EmailCreator emailCreator, IConfiguration configuration, ILogger<EmailQueue> logger)
        {
            _emailCreator = emailCreator;
            _configuration = configuration;
            _logger = logger;
        }

        public void DequeueEmail()
        {

            var connectionString = _configuration.GetConnectionString("MagicFormulaSQLServer"); 
            using var dataBase = new MagicFormulaDbContext(connectionString);
            var emailRequests = dataBase.Set<EmailSolicitacao>()
                .Where(s => !s.Enviado).AsNoTracking().ToList();

            _logger.LogInformation($"Começando a enviar e-mails as: {DateTime.Now}");
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
                catch (Exception ex)
                {
                    _logger.LogError($"Ocorreu um erro ao enviar e-mails: {ex.Message}", ex);
                    throw;
                }

            }
        }

    }
}
