using HtmlAgilityPack;
using M4.Domain.Entensions;
using M4.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace M4.Infrastructure.Services.Http
{
    public class AcoesService : IAcoesService
    {
        private readonly IHttpClientFactory _clientFactory;

        public AcoesService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<Acao>> ObterAcoes()
        {
            HttpClient client = _clientFactory.CreateClient("acoes");
            string response = await client.GetStringAsync(string.Empty);

            List<Acao> acoes = new List<Acao>();
            var doc = new HtmlDocument();
            doc.LoadHtml(response.Replace("%", ""));
            List<List<string>> table = ExtrairDados(doc);

            foreach (var item in table)
            {
                Acao acao = new Acao(ticker: item[0],
                    cotacao: item[1].ToDecimal(),
                    pL: item[2].ToDecimal(),
                    pVP: item[3].ToDecimal(),
                    pSR: item[4].ToDecimal(),
                    dY: item[5].ToDecimal(),
                    pAtivo: item[6].ToDecimal(),
                    pCapGiro: item[7].ToDecimal(),
                    pEBIT: item[8].ToDecimal(),
                    pAtivoCirculanteLiquido: item[9].ToDecimal(),
                    eVEBIT: item[10].ToDecimal(),
                    eVEBITDA: item[11].ToDecimal(),
                    margemEbit: item[12].ToDecimal(),
                    margemLiquida: item[13].ToDecimal(),
                    liquidezCorrente: item[14].ToDecimal(),
                    rOIC: item[15].ToDecimal(),
                    rOE: item[16].ToDecimal(),
                    liquidez2Meses: item[17].ToDecimal(),
                    patrimonioLiquido: item[18].ToDecimal(),
                    divBrutaPatrimonio: item[19].ToDecimal(),
                    crescimentoReceita5Anos: item[20].ToDecimal());

                acoes.Add(acao);
            }
            return await Task.FromResult(acoes.OrderBy(x => x.Ticker));
        }

        private static List<List<string>> ExtrairDados(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectSingleNode("//table")
            .Descendants("tr").Skip(1)
            .Where(tr => tr.Elements("td").Count() > 1)
            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
            .ToList();
        }
    }
}
