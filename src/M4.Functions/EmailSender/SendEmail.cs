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
            ILogger logger = context.GetLogger("SendEmail");
            try
            {
                logger.LogDebug($"Executando rotina as: {DateTime.Now}");
                await _EmailQueue.DequeueEmailAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Ocorreu o erro: {ex.Message} as: {DateTime.Now} ao tentar processar a função");
            }
            finally
            {
                logger.LogInformation($"Proxima execução as: {myTimer.ScheduleStatus.Next}");
            }

        }
    }
}
