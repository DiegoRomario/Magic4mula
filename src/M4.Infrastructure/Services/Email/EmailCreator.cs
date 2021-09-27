using M4.Domain.Core;
using M4.Infrastructure.Configurations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace M4.Infrastructure.Services.Email
{
    public class EmailCreator : IEmailCreator
    {
        private readonly IConfiguration _configuration;
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailCreator> _logger;
        public EmailCreator(IOptions<EmailConfiguration> settings, ILogger<EmailCreator> logger, IConfiguration configuration)
        {
            _emailConfiguration = settings.Value;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task SendEmail(string subject, string message, string to)
        {
            _logger.LogInformation($"Enviando e-mail para: {to} com assunto: {subject} as: {DateTime.Now}");

            try
            {
                string apiKey = _configuration.GetConnectionString("SendGrid");
                SendGridClient client = new (apiKey);
                EmailAddress from = new (_emailConfiguration.From, _emailConfiguration.UserName);
                string subjectEmail = $"🧙 Magic4mula - {subject} 🧙";
                EmailAddress toEmail = new (to);
                string htmlContent = string.Format("<h2 style='color:blue;'>{0}</h2>", message);
                SendGridMessage msg = MailHelper.CreateSingleEmail(from, toEmail, subjectEmail, string.Empty, htmlContent);
                Response response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao enviar e-mail: {ex.Message}", ex);
                throw;
            }

        }
    }
}
