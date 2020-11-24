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
        [Authorize]
        public async Task<ActionResult<IEnumerable<Acao>>> ObterTodas()
        {
            IEnumerable<Acao> result = await ObterAcoesCache();
            return BaseResponse(result);
        }

        [HttpGet("obter-todas-m4")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AcaoClassificacao>>> ObterTodasM4([FromQuery] ECriterio criterio)
        {
            var acoes = await ObterAcoesClassificadas(criterio);
            return BaseResponse(acoes);
        }

        [HttpGet("obter-5-m4")]
        public async Task<ActionResult<IEnumerable<AcaoClassificacao>>> Obter5M4([FromQuery] ECriterio criterio)
        {
            var acoes = await ObterAcoesClassificadas(criterio);
            return BaseResponse(acoes.Take(5));
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
        private async Task<IEnumerable<AcaoClassificacao>> ObterAcoesClassificadas(ECriterio criterio)
        {
            IEnumerable<Acao> acoes = await ObterAcoesCache();
            IEnumerable<AcaoClassificacao> acoesClassificadas = _mapper.Map<IEnumerable<AcaoClassificacao>>(acoes);

            acoesClassificadas = ClassificarPorPreco(criterio, acoesClassificadas);

            int pontuacao = 1;
            foreach (var acao in acoesClassificadas)
            {
                RankearPorPreco(pontuacao, acao, criterio);
                pontuacao++;
            }

            acoesClassificadas = ClassificarPorRentabilidade(criterio, acoesClassificadas);

            pontuacao = 1;
            foreach (var acao in acoesClassificadas)
            {
                RankearPorRentabilidade(pontuacao, acao, criterio);
                acao.Pontuacao = (acao.PLPontuacao + acao.ROEPontuacao) + (acao.EVEBITPontuacao + acao.ROICPontuacao) ;
                pontuacao++;
            }

            acoesClassificadas = acoesClassificadas.OrderByDescending(a => a.Pontuacao);
            pontuacao = 1;
            foreach (var acao in acoesClassificadas)
            {
                acao.Pontuacao = pontuacao;
                pontuacao++;
            }

            IOrderedEnumerable<AcaoClassificacao> resultado = acoesClassificadas.OrderBy(a => a.Pontuacao);

            return await Task.FromResult(resultado);
        }
        private static void RankearPorRentabilidade(int pontuacao, AcaoClassificacao acao, ECriterio criterio)
        {
            if (criterio == ECriterio.PL_ROE)
            {
                acao.ROEPontuacao = pontuacao;
                acao.ROICPontuacao = 0;
            }
            else
            {
                acao.ROICPontuacao = pontuacao;
                acao.ROEPontuacao = 0;
            }

        }
        private static void RankearPorPreco(int pontuacao, AcaoClassificacao acao, ECriterio criterio)
        {
            if (criterio == ECriterio.PL_ROE)
            {
                acao.PLPontuacao = pontuacao;
                acao.EVEBITPontuacao = 0;
            }
            else
            {
                acao.EVEBITPontuacao = pontuacao;
                acao.PLPontuacao = 0;
            }

        }
        private static IEnumerable<AcaoClassificacao> ClassificarPorRentabilidade(ECriterio criterio, IEnumerable<AcaoClassificacao> acoesClassificadas)
        {
            if (criterio == ECriterio.PL_ROE)
                acoesClassificadas = acoesClassificadas.Where(a => a.ROE > 0).OrderBy(a => a.ROE);
            else
                acoesClassificadas = acoesClassificadas.Where(a => a.ROIC > 0).OrderBy(a => a.ROIC);
            return acoesClassificadas;
        }
        private static IEnumerable<AcaoClassificacao> ClassificarPorPreco(ECriterio criterio, IEnumerable<AcaoClassificacao> acoesClassificadas)
        {
            if (criterio == ECriterio.PL_ROE)
                acoesClassificadas = acoesClassificadas.Where(a => a.PL > 0).OrderByDescending(a => a.PL);
            else
                acoesClassificadas = acoesClassificadas.Where(a => a.EVEBIT > 0).OrderByDescending(a => a.EVEBIT);
            return acoesClassificadas;
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
