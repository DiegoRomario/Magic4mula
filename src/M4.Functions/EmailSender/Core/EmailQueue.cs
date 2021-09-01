using M4.Infrastructure.Data.Context;
using M4.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EmailSender.Core
{
    public class EmailQueue
    {
        private readonly DbContextOptions<MagicFormulaDbContext> _optionsBuilder;
        private readonly EmailCreator _emailCreator;
        private readonly ILogger<EmailQueue> _logger;

        public EmailQueue(EmailCreator emailCreator, ILogger<EmailQueue> logger)
        {
            _optionsBuilder = new DbContextOptions<MagicFormulaDbContext>();
            _emailCreator = emailCreator;
            _logger = logger;
        }

        public void DequeueEmail()
        {
            using var dataBase = new MagicFormulaDbContext(_optionsBuilder);

            var emailRequests = dataBase.Set<EmailSolicitacao>()
                .Where(s => !s.Enviado).AsNoTracking().ToList();

            foreach (var request in emailRequests)
            {
                try
                {
                    _emailCreator.SendEmail(request.Titulo, request.Mensagem,
                        request.Destinatarios);
                    request.DataEnvio = DateTime.Now;
                    request.Enviado = true;
                    dataBase.EmailSolicitacao.Update(request);
                    dataBase.SaveChanges();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ocorreu um erro ao tentar enviar o item {request.Id} da fila de e-mail: " + ex.Message);
                    throw;
                }

            }
        }

    }
}
