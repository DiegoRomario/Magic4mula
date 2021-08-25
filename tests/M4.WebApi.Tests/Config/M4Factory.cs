using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace M4.WebApi.Tests.Config
{
    public class M4Factory<Startup> : WebApplicationFactory<Startup> where Startup : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                          .UseStartup<Startup>()
                          .UseEnvironment("Testing");
        }
    }
}
