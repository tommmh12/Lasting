using Lasting.Models;

namespace Lasting.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, ShippingAddress shippingAddress);
        Task<Order> GetOrderDetailsAsync(int orderId);
    }
}
