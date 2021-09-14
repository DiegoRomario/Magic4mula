using M4.Domain.Core;
using M4.Domain.Entities;
using M4.Domain.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Email
{
    public class EmailQueue : IEmailQueue
    {
        private const string QUEUE_NAME = "email-queue";
        private readonly IEmailCreator _emailCreator;
        private readonly ILogger<EmailQueue> _logger;
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queueClient;


        public EmailQueue(IEmailCreator emailCreator, ILogger<EmailQueue> logger, IConfiguration configuration)
        {
            _emailCreator = emailCreator;
            _logger = logger;
            _configuration = configuration;
            var ConnectionStringBus = _configuration.GetConnectionString("MagicFormulaServiceBus");
            _queueClient = new QueueClient(ConnectionStringBus, QUEUE_NAME);
        }
        public async Task EnqueueEmailAsync(EmailSolicitacao emailSolicitacao)
        {
            try
            {
                string emailSolicitacaoJson = JsonSerializer.Serialize(emailSolicitacao);
                byte[] emailSolicitacaoBytes = Encoding.UTF8.GetBytes(emailSolicitacaoJson);
                Message message = new(emailSolicitacaoBytes);
                _logger.LogInformation($"Enfileirando E-mail Id: {emailSolicitacao.Id}");
                await _queueClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao adicionar e-mail Id: {emailSolicitacao.Id} a fila: {ex.Message}", ex);
            }

        }

       public async Task DequeueEmailAsync()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionHandler)
            {
                AutoComplete = false
            };
            _queueClient.RegisterMessageHandler(ProcessMessageHandler, messageHandlerOptions);
            await Task.CompletedTask;
        }

        private async Task ProcessMessageHandler(Message message, CancellationToken cancellationToken)
        {
            var messageString = Encoding.UTF8.GetString(message.Body);

            EmailSolicitacao emailSolicitacao = JsonSerializer.Deserialize<EmailSolicitacao>(messageString);
            try
            {
                await _emailCreator.SendEmail(emailSolicitacao.Titulo, emailSolicitacao.Mensagem, emailSolicitacao.Destinatarios);
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao enviar e-mail: {ex.Message}", ex);
            }

        }
        private async Task ExceptionHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Ocorreu um erro ao tentar consumir mensagem da fila de e-mail: {exceptionReceivedEventArgs.Exception.Message}", exceptionReceivedEventArgs.Exception);
            await Task.CompletedTask;
        }

    }
}
