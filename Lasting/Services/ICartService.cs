using Lasting.Models;

namespace Lasting.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity = 1);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task UpdateQuantityAsync(string userId, int cartItemId, int newQuantity);
        Task ClearCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<int> GetCartCountAsync(string userId);
    }
}