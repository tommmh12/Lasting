using System.ComponentModel.DataAnnotations;

namespace Lasting.Models
{
    public class CartItem
    {
        [Key]   
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}