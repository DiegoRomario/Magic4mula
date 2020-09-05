using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using M4.Infrastructure.Services.Http;
using M4.Domain.Entities;
using System.Collections.Generic;

namespace M4.WebApi.Controllers
{
    [Route("api/acoes")]
    public class AcaoController : BaseController
    {
        private readonly IAcoesService _acoesService;

        public AcaoController(IAcoesService acoesService)
        {
            _acoesService = acoesService;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Acao>>> ObterAcoes ()
        {
            IEnumerable<Acao> result = await _acoesService.ObterAcoes();
            return Ok(result);
        }
    }
}
