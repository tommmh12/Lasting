namespace Lasting.Models
{
    public class ReviewReply
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public ProductReview Review { get; set; }

        public string Content { get; set; } = "";
        public string? AdminId { get; set; }
        public ApplicationUser? Admin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
