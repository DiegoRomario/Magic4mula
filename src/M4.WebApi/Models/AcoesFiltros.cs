using System.ComponentModel;

namespace M4.WebApi.Models
{
    public class AcoesFiltros
    {
        private const int VALOR_MAXIMO = 99999;
        private const int VALOR_MINIMO = VALOR_MAXIMO * -1;

        public Direction Direction { get; set; }
        public string OrderBy { get; set; } = "Pontuacao";
        // menor
        public decimal PL { get; set; } = VALOR_MAXIMO;
        public decimal PVP { get; set; } = VALOR_MAXIMO;
        public decimal EVEBIT { get; set; } = VALOR_MAXIMO;
        public decimal EVEBITDA { get; set; } = VALOR_MAXIMO;
        // maior
        public decimal DY { get; set; } = VALOR_MINIMO;
        public decimal MargemEbit { get; set; } = VALOR_MINIMO;
        public decimal MargemLiquida { get; set; } = VALOR_MINIMO;
        public decimal ROIC { get; set; } = VALOR_MINIMO;
        public decimal ROE { get; set; } = VALOR_MINIMO;
        public decimal Liquidez2Meses { get; set; } = VALOR_MINIMO;
        public decimal PatrimonioLiquido { get; set; } = VALOR_MINIMO;
        public decimal CrescimentoReceita5Anos { get; set; } = VALOR_MINIMO;
    }

    //[DefaultValue(ASC)]
    public enum Direction
    {
        ASC,
        DESC
    }

}
