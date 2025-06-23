using System.ComponentModel.DataAnnotations;

namespace Lasting.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; } // ID của CartItem

        public int ProductId { get; set; } // ID sản phẩm

        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Giá")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Thành tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice => Price * Quantity;
    }
}