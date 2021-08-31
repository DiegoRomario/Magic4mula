using Microsoft.AspNetCore.Identity;

namespace M4.Infrastructure.Data.Models
{
    public class UserIdentity : IdentityUser
    {
        public string Name { get; set; }
        public string LastName { get; set; }

    }
}
