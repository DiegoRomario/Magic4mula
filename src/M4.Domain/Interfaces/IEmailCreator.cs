using System.Threading.Tasks;

namespace M4.Domain.Core
{
    public interface IEmailCreator
    {
        Task SendEmail(string subject, string message, string toName, string toEmail);
    }
}