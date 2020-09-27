using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace M4.Infrastructure.Data.Identity
{
    public class UserIdentityDbContext : IdentityDbContext<UserIdentity>
    {
        public UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserIdentity>().ToTable("UserIdentity");
            builder.Entity<IdentityRole>().ToTable("Role");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken"); 
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");

        }
    }
}
