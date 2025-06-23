using Lasting.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lasting.Services
{
    public interface ICartService
    {
        // Cho người dùng đã đăng nhập
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int quantity = 1);
        Task RemoveFromCartAsync(string userId, int cartItemId);
        Task UpdateQuantityAsync(string userId, int cartItemId, int newQuantity);
        Task ClearCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<int> GetCartCountAsync(string userId);

        // Cho khách (dùng session)
        Task<List<TempCartItem>> GetTempCartAsync();
        Task<int> GetTempCartCountAsync();
        Task<decimal> GetTempCartTotalAsync();
        Task UpdateTempCartItemAsync(int productId, int quantity);
        Task RemoveTempCartItemAsync(int productId);
        Task MergeCartAsync(string userId); // Hợp nhất giỏ hàng khi đăng nhập
    }
}