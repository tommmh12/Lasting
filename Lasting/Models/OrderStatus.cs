namespace Lasting.Models
{
    public enum OrderStatus
    {
        Pending,      // Đã đặt
        Processing,   // Đang xử lý
        Shipped,      // Đã gửi đi
        Delivered,    // Đã giao thành công
        Cancelled     // Đã hủy
    }
}
