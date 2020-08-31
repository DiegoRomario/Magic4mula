using System.Threading.Tasks;

namespace M4.Infrastructure.Email
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message);
        Task SendEmailAsync(EmailMessage message);
    }
}
