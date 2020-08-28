using Microsoft.EntityFrameworkCore;
using Identity = Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace M4.Infrastructure.Data
{
    public class MyIdentityDbContext : Identity.IdentityDbContext
    {
        public MyIdentityDbContext(DbContextOptions<MyIdentityDbContext> options) : base(options) { }
    }
}
