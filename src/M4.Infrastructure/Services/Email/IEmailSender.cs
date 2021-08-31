using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
