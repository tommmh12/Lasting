using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Lasting.Models
{
        public class ApplicationUser : IdentityUser
        {
        [Required]
        [PersonalData]
        public string? FullName { get; set; }

        [PersonalData]
        [StringLength(255)]
        public string? Address { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string? City { get; set; }
        [PersonalData]
        public String? PhoneNumber { get; set; }

        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        public string? Gender { get; set; }

        [PersonalData]
        public string? Province { get; set; }

        [PersonalData]
        public string? District { get; set; }

        [PersonalData]
        public string? Ward { get; set; }
    }
}
