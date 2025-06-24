using System.ComponentModel.DataAnnotations;

namespace Lasting.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
