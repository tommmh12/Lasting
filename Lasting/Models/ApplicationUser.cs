using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Lasting.Models
{
        public class ApplicationUser : IdentityUser
        {
            [Required]
            [PersonalData]
            public string? FullName { get; set; }
        }
}
