using M4.Domain.Core;
using M4.Infrastructure.Configurations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Email
{
    public class EmailCreator : IEmailCreator
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailCreator> _logger;
        public EmailCreator(IOptions<EmailConfiguration> settings, ILogger<EmailCreator> logger)
        {
            _emailConfiguration = settings.Value;
            _logger = logger;
        }
        public async Task SendEmail(string subject, string message, string to)
        {
            _logger.LogInformation($"Enviando e-mail para: {to} com assunto: {subject} as: {DateTime.Now}");
            MailMessage mailMessage = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                MailAddress fromAddress = new MailAddress(_emailConfiguration.From);
                mailMessage.From = fromAddress;
                foreach (var item in to.Split(";"))
                    mailMessage.To.Add(item);

                mailMessage.Subject = $"🧙 Magic4mula - {subject} 🧙";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = string.Format("<h2 style='color:blue;'>{0}</h2>", message);
                smtpClient.Host = _emailConfiguration.SmtpServer;
                smtpClient.Port = _emailConfiguration.Port;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao enviar e-mail: {ex.Message}", ex);
                throw;
            }

        }
    }
}
