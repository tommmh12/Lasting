using System.ComponentModel.DataAnnotations;

namespace Lasting.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = "";

        public string? AdminReply { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RepliedAt { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }

        public List<ReviewImage> Images { get; set; } = new();
        public List<ReviewReply> Replies { get; set; } = new();
    }
}
