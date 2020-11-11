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
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;

namespace M4.WebApi.Controllers
{
    [Route("api/acoes")]
    [Authorize]
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

        [HttpGet("obter-todas-m4")]
        public async Task<ActionResult<IEnumerable<AcaoClassificacao>>> ObterTodasGreenblatt([FromQuery] AcoesFiltros filtros)
        {
            var acoesGreenblatt = await ObterAcoesModeloGreenblattOriginal();
            var resultado = FiltrarAcoes(acoesGreenblatt, filtros);
            return BaseResponse(resultado.AsQueryable().OrderBy($"{filtros.OrderBy} { filtros.Direction}"));
        }

        private async Task<IEnumerable<Acao>> ObterAcoesCache()
        {
            IEnumerable<Acao> acoes = await _cache.GetOrCreateAsync(_TODASACOES, async func =>
            {
                func.SetAbsoluteExpiration(TimeSpan.FromHours(12));
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
        private IEnumerable<AcaoClassificacao> FiltrarAcoes(IEnumerable<AcaoClassificacao> acoes, AcoesFiltros filtros)
        {
            return acoes.Where(x => Filtros(x, filtros));
        }
        Func<AcaoClassificacao, AcoesFiltros, bool> Filtros = delegate (AcaoClassificacao acoes, AcoesFiltros filtros)
        {
            return acoes.PL < filtros.PL &&
                    acoes.PVP < filtros.PVP &&
                    acoes.EVEBIT < filtros.EVEBIT &&
                    acoes.EVEBITDA < filtros.EVEBITDA &&

                    acoes.DY > filtros.DY &&
                    acoes.MargemEbit > filtros.MargemEbit &&
                    acoes.MargemLiquida > filtros.MargemLiquida &&
                    acoes.ROIC > filtros.ROIC &&
                    acoes.ROE > filtros.ROE &&
                    acoes.Liquidez2Meses > filtros.Liquidez2Meses &&
                    acoes.PatrimonioLiquido > filtros.PatrimonioLiquido &&
                    acoes.CrescimentoReceita5Anos > filtros.CrescimentoReceita5Anos;
        };

    }
}
