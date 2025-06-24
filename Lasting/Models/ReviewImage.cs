using System.ComponentModel.DataAnnotations;

namespace Lasting.Models
{
    public class ReviewImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public ProductReview Review { get; set; } = null!;
    }
}
