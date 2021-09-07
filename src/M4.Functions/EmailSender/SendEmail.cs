using System;
using System.Threading.Tasks;
using M4.Domain.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EmailSender
{
    public class SendEmail
    {
        private readonly IEmailQueue _EmailQueue;
        public SendEmail(IEmailQueue emailQueue)
        {
            this._EmailQueue = emailQueue;
        }
        [FunctionName("SendEmail")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Iniciando função para envio de e-mails as: {DateTime.Now}");
            await _EmailQueue.DequeueEmailAsync();
        }
    }
}
