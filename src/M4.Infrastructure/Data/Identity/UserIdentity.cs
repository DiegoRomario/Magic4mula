using Microsoft.AspNetCore.Identity;

namespace M4.Infrastructure.Data.Identity
{
    public class UserIdentity : IdentityUser
    {
        public string Name { get; set; }
        public string LastName { get; set; }

    }
}
