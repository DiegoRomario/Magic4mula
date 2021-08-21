using Microsoft.Extensions.Configuration;

namespace M4.WebApi.Tests.Config
{
    public class StartupTesting : Startup
    {
        public StartupTesting(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
