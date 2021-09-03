using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EmailSender.Core
{
    public class EmailCreator
    {
        private readonly EmailConfiguration _emailConfiguration;
        public EmailCreator(IOptions<EmailConfiguration> settings)
        {
            _emailConfiguration = settings.Value;
        }
        public bool SendEmail(string subject, string message, string to)
        {

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
                mailMessage.IsBodyHtml = true;
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
            catch
            {
                throw;
            }

        }
    }
}
