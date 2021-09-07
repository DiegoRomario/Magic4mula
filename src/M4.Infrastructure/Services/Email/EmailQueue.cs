using M4.Domain.Core;
using M4.Domain.Entities;
using M4.Domain.Interfaces;
using M4.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Email
{
    public class EmailQueue : IEmailQueue
    {
        private readonly IEmailCreator _emailCreator;
        private readonly ILogger<EmailQueue> _logger;
        private readonly MagicFormulaDbContext _magicFormulaDbContext;

        public EmailQueue(IEmailCreator emailCreator, ILogger<EmailQueue> logger, MagicFormulaDbContext magicFormulaDbContext)
        {
            _emailCreator = emailCreator;
            _logger = logger;
            _magicFormulaDbContext = magicFormulaDbContext;
        }

        public async Task DequeueEmailAsync()
        {
            IList<EmailSolicitacao> emailRequests = await GetUnqueuedEmails();

            _logger.LogInformation($"Existem {emailRequests.Count} para serem enviados: {DateTime.Now}");
            foreach (var request in emailRequests)
            {
                try
                {
                    await _emailCreator.SendEmail(request.Titulo, request.Mensagem, request.Destinatarios);
                    request.MarcarEmailComoEnviado();
                    _magicFormulaDbContext.EmailSolicitacao.Update(request);
                    await _magicFormulaDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ocorreu um erro ao enviar e-mails: {ex.Message}", ex);
                    throw;
                }

            }
        }

        public async Task EnqueueEmailAsync(EmailSolicitacao emailSolicitacao)
        {
            try
            {
                _logger.LogInformation($"Enfileirando E-mail Id: {emailSolicitacao.Id}");
                await _magicFormulaDbContext.AddAsync(emailSolicitacao);
                await _magicFormulaDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao adicionar e-mail Id: {emailSolicitacao.Id} a fila: {ex.Message}", ex);
                throw;
            }

        }

        public async Task<IList<EmailSolicitacao>> GetUnqueuedEmails()
        {
            return await _magicFormulaDbContext.Set<EmailSolicitacao>()
                .Where(s => !s.Enviado).AsNoTracking().ToListAsync();
        }

    }
}
