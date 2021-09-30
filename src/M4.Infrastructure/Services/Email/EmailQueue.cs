using Azure.Messaging.ServiceBus;
using M4.Domain.Core;
using M4.Domain.Entities;
using M4.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace M4.Infrastructure.Services.Email
{
    public class EmailQueue : IEmailQueue
    {
        private const string QUEUE_NAME = "queue-email";
        private readonly IEmailCreator _emailCreator;
        private readonly ILogger<EmailQueue> _logger;
        private readonly IConfiguration _configuration;
        private ServiceBusClient _serviceBusClient;


        public EmailQueue(IEmailCreator emailCreator, ILogger<EmailQueue> logger, IConfiguration configuration)
        {
            _emailCreator = emailCreator;
            _logger = logger;
            _configuration = configuration;
        }

        private void GetServiceBusClient()
        {
            var cs = _configuration.GetConnectionString("MagicFormulaServiceBus");
            _serviceBusClient = new(cs);
        }

        public async Task EnqueueEmailAsync(EmailSolicitacao emailSolicitacao)
        {

            try
            {
                GetServiceBusClient();
                ServiceBusSender sender = _serviceBusClient.CreateSender(QUEUE_NAME);
                string emailSolicitacaoJson = JsonSerializer.Serialize(emailSolicitacao);
                byte[] emailSolicitacaoBytes = Encoding.UTF8.GetBytes(emailSolicitacaoJson);
                ServiceBusMessage message = new(emailSolicitacaoBytes);
                _logger.LogInformation($"Enfileirando E-mail Id: {emailSolicitacao.Id}");
                await sender.SendMessageAsync(message);
            }
            catch
            {
                throw;
            }

        }

        public async Task DequeueEmailAsync()
        {
            try
            {
                GetServiceBusClient();
                ServiceBusReceiver receiver = _serviceBusClient.CreateReceiver(QUEUE_NAME);
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync();
                if (message is not null)
                {
                    using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                    var messageString = Encoding.UTF8.GetString(message.Body);
                    EmailSolicitacao emailSolicitacao = JsonSerializer.Deserialize<EmailSolicitacao>(messageString);
                    await _emailCreator.SendEmail(emailSolicitacao.Titulo, emailSolicitacao.Mensagem, emailSolicitacao.NomeDestinatario, emailSolicitacao.EmailDestinatario);
                    await receiver.CompleteMessageAsync(message);
                    scope.Complete();
                }
                else await Task.CompletedTask;
            }
            catch
            {
                throw;
            }

        }
    }
}
