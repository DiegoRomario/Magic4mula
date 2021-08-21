using M4.Domain.Entities;


namespace M4.WebApi.Models
{
    public class AcaoClassificacao : Acao
    {
        protected AcaoClassificacao() { }
        public AcaoClassificacao(string ticker, decimal cotacao, decimal pL, decimal pVP, decimal pSR, decimal dY, decimal pAtivo, 
            decimal pCapGiro, decimal pEBIT, decimal pAtivoCirculanteLiquido, decimal eVEBIT, decimal eVEBITDA, decimal margemEbit, 
            decimal margemLiquida, decimal liquidezCorrente, decimal rOIC, decimal rOE, decimal liquidez2Meses, 
            decimal patrimonioLiquido, decimal divBrutaPatrimonio, decimal crescimentoReceita5Anos)
            :
            base (ticker, cotacao, pL, pVP, pSR, dY, pAtivo, pCapGiro, pEBIT, pAtivoCirculanteLiquido, eVEBIT, eVEBITDA, margemEbit, 
                  margemLiquida, liquidezCorrente, rOIC, rOE, liquidez2Meses, patrimonioLiquido, divBrutaPatrimonio, crescimentoReceita5Anos)
        {
        }
        public int Pontuacao { get; set; }
        public int PLPontuacao { get; set; }
        public int ROEPontuacao { get; set; }
        public int EVEBITPontuacao { get; set; }
        public int ROICPontuacao { get; set; }
    }

}
