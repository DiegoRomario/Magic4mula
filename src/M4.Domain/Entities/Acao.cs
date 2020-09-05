namespace M4.Domain.Entities
{
    public class Acao : Entity
    {
        private Acao() {}
        public Acao(string ticker, decimal cotacao, string pL, decimal pVP, decimal pSR, decimal dY, decimal pAtivo, decimal pCapGiro, decimal pEBIT, decimal pAtivoCirculanteLiquido, decimal eVEBIT, decimal eVEBITDA, decimal margemEbit, decimal margemLiquida, decimal liquidezCorrente, decimal rOIC, decimal rOE, decimal liquidez2Meses, decimal patrimonioLiquido, decimal divBrutaPatrimonio, decimal crescimentoReceita5Anos)
        {
            Ticker = ticker;
            Cotacao = cotacao;
            PL = pL;
            PVP = pVP;
            PSR = pSR;
            DY = dY;
            PAtivo = pAtivo;
            PCapGiro = pCapGiro;
            PEBIT = pEBIT;
            PAtivoCirculanteLiquido = pAtivoCirculanteLiquido;
            EVEBIT = eVEBIT;
            EVEBITDA = eVEBITDA;
            MargemEbit = margemEbit;
            MargemLiquida = margemLiquida;
            LiquidezCorrente = liquidezCorrente;
            ROIC = rOIC;
            ROE = rOE;
            Liquidez2Meses = liquidez2Meses;
            PatrimonioLiquido = patrimonioLiquido;
            DivBrutaPatrimonio = divBrutaPatrimonio;
            CrescimentoReceita5Anos = crescimentoReceita5Anos;
        }

        public string Ticker { get; private set; }
        public decimal Cotacao { get; private set; }
        public string PL { get; private set; }
        public decimal PVP { get; private set; }
        public decimal PSR { get; private set; }
        public decimal DY { get; private set; }
        public decimal PAtivo { get; private set; }
        public decimal PCapGiro { get; private set; }
        public decimal PEBIT { get; private set; }
        public decimal PAtivoCirculanteLiquido { get; private set; }
        public decimal EVEBIT { get; private set; }
        public decimal EVEBITDA { get; private set; }
        public decimal MargemEbit { get; private set; }
        public decimal MargemLiquida { get; private set; }
        public decimal LiquidezCorrente { get; private set; }
        public decimal ROIC { get; private set; }
        public decimal ROE { get; private set; }
        public decimal Liquidez2Meses { get; private set; }
        public decimal PatrimonioLiquido { get; private set; }
        public decimal DivBrutaPatrimonio { get; private set; }
        public decimal CrescimentoReceita5Anos { get; private set; }
    }
}
