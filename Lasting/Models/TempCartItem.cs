namespace Lasting.Models
{
    public class TempCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        // Thêm các thông tin khác nếu cần
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
