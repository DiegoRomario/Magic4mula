using System;
using System.Threading.Tasks;
using EmailSender.Models;
using M4.Domain.Interfaces;
using Microsoft.Azure.Functions.Worker;
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
        [Function("SendEmail")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] MyInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("SendEmail");
            logger.LogInformation($"Função executada as: {DateTime.Now}");            
            await _EmailQueue.DequeueEmailAsync();
            logger.LogInformation($"Proxima execução as: {myTimer.ScheduleStatus.Next}");
        }
    }
}
