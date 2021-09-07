using M4.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M4.Domain.Interfaces
{
    public interface IEmailQueue
    {
        Task DequeueEmailAsync();
        Task EnqueueEmailAsync(EmailSolicitacao emailSolicitacao);
        Task<IList<EmailSolicitacao>> GetUnqueuedEmails();
    }
}
