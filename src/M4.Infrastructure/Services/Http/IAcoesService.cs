using M4.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Http
{
    public interface IAcoesService
    {
        Task<IEnumerable<Acao>> ObterAcoes();
    }
}