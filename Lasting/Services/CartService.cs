using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Lasting.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lasting.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public CartService(ApplicationDbContext context,
                         IHttpContextAccessor httpContextAccessor,
                         IProductService productService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        #region Private Methods
        private List<TempCartItem> GetTempCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return session.Get<List<TempCartItem>>("TempCart") ?? new List<TempCartItem>();
        }

        private void SaveTempCart(List<TempCartItem> tempCart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Set("TempCart", tempCart);
        }
        #endregion

        #region Authenticated User Methods
        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ThenInclude(p => p.Brand)
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity = 1)
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(newItem);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(string userId, int cartItemId, int newQuantity)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item != null)
            {
                item.Quantity = newQuantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .SumAsync(c => c.Product.Price * c.Quantity);
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Guest User Methods
        public async Task<List<TempCartItem>> GetTempCartAsync()
        {
            return GetTempCart();
        }

        public async Task<int> GetTempCartCountAsync()
        {
            var tempCart = GetTempCart();
            return tempCart.Sum(item => item.Quantity);
        }

        public async Task<decimal> GetTempCartTotalAsync()
        {
            var tempCart = GetTempCart();
            return tempCart.Sum(item => item.Price * item.Quantity);
        }

        public async Task UpdateTempCartItemAsync(int productId, int quantity)
        {
            var tempCart = GetTempCart();
            var item = tempCart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveTempCart(tempCart);
            }
        }

        public async Task RemoveTempCartItemAsync(int productId)
        {
            var tempCart = GetTempCart();
            tempCart.RemoveAll(x => x.ProductId == productId);
            SaveTempCart(tempCart);
        }
        #endregion

        #region Merge Cart
        public async Task MergeCartAsync(string userId)
        {
            var tempCart = GetTempCart();
            if (tempCart.Any())
            {
                foreach (var item in tempCart)
                {
                    await AddToCartAsync(userId, item.ProductId, item.Quantity);
                }

                // Xóa giỏ hàng tạm sau khi hợp nhất
                SaveTempCart(new List<TempCartItem>());
            }
        }
        #endregion
    }
}