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
        private readonly MagicFormulaDbContext _magicFormulaDbContext;

        public EmailQueue(EmailCreator emailCreator, IConfiguration configuration, ILogger<EmailQueue> logger, MagicFormulaDbContext magicFormulaDbContext)
        {
            _emailCreator = emailCreator;
            _configuration = configuration;
            _logger = logger;
            _magicFormulaDbContext = magicFormulaDbContext;
        }

        public void DequeueEmail()
        {
            var emailRequests = _magicFormulaDbContext.Set<EmailSolicitacao>()
                .Where(s => !s.Enviado).AsNoTracking().ToList();

            _logger.LogInformation($"Existem {emailRequests.Count} para serem enviados: {DateTime.Now}");
            foreach (var request in emailRequests)
            {
                try
                {
                    _emailCreator.SendEmail(request.Titulo, request.Mensagem, request.Destinatarios);
                    request.DataEnvio = DateTime.Now;
                    request.Enviado = true;
                    _magicFormulaDbContext.EmailSolicitacao.Update(request);
                    _magicFormulaDbContext.SaveChanges();
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
