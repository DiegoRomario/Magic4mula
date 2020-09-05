using AutoMapper;
using M4.Domain.Entities;
using M4.Infrastructure.Services.Http;
using M4.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M4.WebApi.Controllers
{
    [Route("api/acoes")]
    public class AcaoController : BaseController
    {
        private readonly IAcoesService _acoesService;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private const string _TODASACOES = "todasacoes";

        public AcaoController(IAcoesService acoesService, IMemoryCache cache, IMapper mapper)
        {
            _acoesService = acoesService;
            _cache = cache;
            _mapper = mapper;
        }

        [HttpGet("obter-todas")]
        public async Task<ActionResult<IEnumerable<Acao>>> ObterTodas()
        {
            IEnumerable<Acao> result = await ObterAcoesCache();
            return BaseResponse(result);
        }

        [HttpGet("obter-todas-greenblatt-original")]
        public async Task<ActionResult<IEnumerable<AcaoClassificacao>>> ObterTodasGreenblatt()
        {
            IEnumerable<AcaoClassificacao> result = await ObterAcoesModeloGreenblattOriginal();
            return BaseResponse(result);
        }

        private async Task<IEnumerable<Acao>> ObterAcoesCache()
        {
            IEnumerable<Acao> acoes = await _cache.GetOrCreateAsync(_TODASACOES, async func =>
            {
                func.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                return await _acoesService.ObterAcoes();
            });

            return await Task.FromResult(acoes);
        }
        private async Task<IEnumerable<AcaoClassificacao>> ObterAcoesModeloGreenblattOriginal()
        {
            IEnumerable<Acao> acoes = await ObterAcoesCache();
            IEnumerable<AcaoClassificacao> acoesClassificadas = _mapper.Map<IEnumerable<AcaoClassificacao>>(acoes);

            acoesClassificadas = acoesClassificadas.Where(a => a.PL > 0).OrderByDescending(a => a.PL);
            int pontuacao = 1;
            foreach (var acao in acoesClassificadas)
            {
                acao.PLPontuacao = pontuacao;
                pontuacao++;
            }
            acoesClassificadas = acoesClassificadas.Where(a => a.ROE > 0).OrderBy(a => a.ROE);
            pontuacao = 1;
            foreach (var acao in acoesClassificadas)
            {
                acao.ROEPontuacao = pontuacao;
                acao.Pontuacao = acao.PLPontuacao + acao.ROEPontuacao;
                pontuacao++;
            }

            IOrderedEnumerable<AcaoClassificacao> resultado = acoesClassificadas.OrderByDescending(a => a.Pontuacao);

            return await Task.FromResult(resultado);
        }
    }
}
