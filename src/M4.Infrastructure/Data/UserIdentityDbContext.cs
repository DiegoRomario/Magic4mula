using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace M4.Infrastructure.Data
{
    public class UserIdentityDbContext : IdentityDbContext
    {
        public UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options) : base(options) { }
    }
}
