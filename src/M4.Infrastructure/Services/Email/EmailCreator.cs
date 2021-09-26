using M4.Domain.Core;
using M4.Infrastructure.Configurations.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

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

            try
            {
                var apiKey = _emailConfiguration.ApiKey;
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_emailConfiguration.From, "Magic4mula");
                var subject2 = $"🧙 Magic4mula - {subject} 🧙";
                var to2 = new EmailAddress(to);
                var plainTextContent = "teste";
                var htmlContent = string.Format("<h2 style='color:blue;'>{0}</h2>", message);
                var msg = MailHelper.CreateSingleEmail(from, to2, subject2, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation(response.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ocorreu um erro ao enviar e-mail: {ex.Message}", ex);
                throw;
            }

        }
    }
}
