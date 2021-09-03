using System;
using EmailSender.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EmailSender
{
    public class SendEmail
    {
        private readonly EmailQueue _EmailQueue;
        public SendEmail(EmailQueue emailQueue)
        {
            this._EmailQueue = emailQueue;
        }
        [FunctionName("SendEmail")]
        public void Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {            
            log.LogInformation($"Iniciando função para envio de e-mails as: {DateTime.Now}");
            _EmailQueue.DequeueEmail();
        }
    }
}