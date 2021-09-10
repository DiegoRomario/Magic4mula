using EmailSender;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EmailSender
{
    public class Startup : FunctionsStartup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {

        }

    }
}