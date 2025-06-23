using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Models
{
    [Owned]
    public class ShippingAddress
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(200)]
        public string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
    }
}
