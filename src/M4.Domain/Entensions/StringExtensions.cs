namespace M4.Domain.Entensions
{
    public static class StringExtensions
    {
        public static decimal ToDecimal(this string valor)
        {
            decimal valorPadrao;
            decimal.TryParse(valor, out valorPadrao);
            return valorPadrao;
        }

    }
}
