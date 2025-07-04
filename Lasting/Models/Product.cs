﻿using System.ComponentModel.DataAnnotations;

namespace Lasting.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OldPrice { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải từ 0 trở lên")]
        public int StockQuantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public List<ProductReview> Reviews { get; set; } = new();
        public List<ProductImage> Gallery { get; set; } = new();

    }
}
