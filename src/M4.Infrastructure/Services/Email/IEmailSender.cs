using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Email
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message);
        Task SendEmailAsync(EmailMessage message);
    }
}
