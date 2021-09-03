using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;

namespace EmailSender.Core
{
    public class EmailCreator
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailCreator> _logger;
        public EmailCreator(IOptions<EmailConfiguration> settings, ILogger<EmailCreator> logger)
        {
            _emailConfiguration = settings.Value;
            _logger = logger;
        }
        public bool SendEmail(string subject, string message, string to)
        {
            _logger.LogInformation($"Criando e-mail as: {DateTime.Now}");
            MailMessage mailMessage = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                MailAddress fromAddress = new MailAddress(_emailConfiguration.From);
                mailMessage.From = fromAddress;
                foreach (var item in to.Split(";"))
                {
                    mailMessage.To.Add(item);
                }
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = false;
                mailMessage.Body = message;
                smtpClient.Host = _emailConfiguration.SmtpServer;
                smtpClient.Port = _emailConfiguration.Port;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials =
                    new NetworkCredential(_emailConfiguration.UserName, _emailConfiguration.Password);
                smtpClient.Send(mailMessage);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao criar e-mail: {ex.Message}", ex);
                throw;
            }

        }
    }
}
