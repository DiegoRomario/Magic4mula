using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;

namespace EmailSender.Core
{
    public class EmailCreator
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailCreator> _logger;

        public EmailCreator(IOptions<EmailConfiguration> emailConfig, ILogger<EmailCreator> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }
        public bool SendEmail(string subject, string message, string to)
        {
            var from = _emailConfig.From;

            MailMessage mailMessage = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                MailAddress fromAddress = new MailAddress(from);
                mailMessage.From = fromAddress;
                foreach (var item in to.Split(";"))
                {
                    mailMessage.To.Add(item);
                }

                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = message;
                smtpClient.Host = _emailConfig.SmtpServer;
                smtpClient.Port = _emailConfig.Port;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials =
                    new System.Net.NetworkCredential(_emailConfig.UserName, _emailConfig.Password);
                smtpClient.Send(mailMessage);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ocorreu um erro ao tentar enviar e-mail para {to}: " + ex.Message);
                throw;
            }

        }
    }
}
