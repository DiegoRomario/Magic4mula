using System.Net.Http;
using System.Net.Http.Headers;

namespace M4.WebApi.Tests.Config
{
    public static class TestsExtensions
    {
        public static void AtribuirToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}


