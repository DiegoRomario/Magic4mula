using System;
using System.Text;
using System.Web;

namespace M4.Domain.Extensions
{
    public static class StringExtensions
    {
        public static decimal ToDecimal(this string valor)
        {
            decimal.TryParse(valor, out decimal valorPadrao);
            return valorPadrao;
        }
        public static string StringToUrlEncoded(string text)
        {
            byte[] textAsBytes = Encoding.ASCII.GetBytes(text);
            string textAsBase64 = Convert.ToBase64String(textAsBytes);
            string textAsUrlEncode = HttpUtility.UrlEncode(textAsBase64);
            return textAsUrlEncode;
        }
        public static string UrlEncodedToString(string textEncoded)
        {
            byte[] textDecodedAsBytes = Convert.FromBase64String(textEncoded);
            string textDecodedAsBase64 = Encoding.ASCII.GetString(textDecodedAsBytes);
            string textDecodedAsString = HttpUtility.UrlDecode(textDecodedAsBase64);
            return textDecodedAsString;
        }

    }
}
