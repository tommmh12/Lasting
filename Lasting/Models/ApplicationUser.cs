using Microsoft.AspNetCore.Identity;

namespace Lasting.Models
{
        public class ApplicationUser : IdentityUser
        {
            public string? FullName { get; set; }
        }
}
