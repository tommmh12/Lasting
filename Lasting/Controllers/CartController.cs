using Lasting.Models;
using Lasting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lasting.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ICartService cartService,
            IProductService productService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = await GetCartModel();
            return View(model.ViewName, model.Items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!await ValidateProduct(productId))
                return NotFound();

            await ProcessAddToCart(productId, quantity);
            return RedirectToCartIndex();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id, bool isTempItem = false)
        {
            await ProcessRemoveFromCart(id, isTempItem);
            return RedirectToCartIndex();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int newQuantity, bool isTempItem = false)
        {
            await ProcessUpdateQuantity(id, newQuantity, isTempItem);
            return RedirectToCartIndex();
        }

        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            var count = await GetCurrentCartCount();
            return Json(new { count });
        }

        [HttpGet]
        public async Task<JsonResult> GetCartTotal()
        {
            var total = await GetCurrentCartTotal();
            return Json(new { total });
        }

        [Authorize]
        public async Task<IActionResult> MergeCart()
        {
            await _cartService.MergeCartAsync(GetCurrentUserId());
            return RedirectToCartIndex();
        }

        #region Private Methods

        private async Task<(object Items, string ViewName)> GetCartModel()
        {
            if (User.Identity.IsAuthenticated)
            {
                var items = await _cartService.GetCartItemsAsync(GetCurrentUserId());
                return (items, "Index");
            }

            var tempItems = await _cartService.GetTempCartAsync();
            return (tempItems, "TempCart");
        }

        private async Task<bool> ValidateProduct(int productId)
        {
            return await _productService.GetProductByIdAsync(productId) != null;
        }

        private async Task ProcessAddToCart(int productId, int quantity)
        {
            var userId = GetCurrentUserId();
            await _cartService.AddToCartAsync(userId, productId, quantity);

            var product = await _productService.GetProductByIdAsync(productId);
            SetSuccessMessage($"Đã thêm {product.Name} vào giỏ hàng");
        }

        private async Task ProcessRemoveFromCart(int id, bool isTempItem)
        {
            if (User.Identity.IsAuthenticated && !isTempItem)
            {
                await _cartService.RemoveFromCartAsync(GetCurrentUserId(), id);
            }
            else
            {
                await _cartService.RemoveTempCartItemAsync(id);
            }
            SetSuccessMessage("Đã xóa sản phẩm khỏi giỏ hàng");
        }

        private async Task ProcessUpdateQuantity(int id, int newQuantity, bool isTempItem)
        {
            if (User.Identity.IsAuthenticated && !isTempItem)
            {
                await _cartService.UpdateQuantityAsync(GetCurrentUserId(), id, newQuantity);
            }
            else
            {
                await _cartService.UpdateTempCartItemAsync(id, newQuantity);
            }
        }

        private async Task<int> GetCurrentCartCount()
        {
            return User.Identity.IsAuthenticated
                ? await _cartService.GetCartCountAsync(GetCurrentUserId())
                : await _cartService.GetTempCartCountAsync();
        }

        private async Task<decimal> GetCurrentCartTotal()
        {
            return User.Identity.IsAuthenticated
                ? await _cartService.GetCartTotalAsync(GetCurrentUserId())
                : await _cartService.GetTempCartTotalAsync();
        }


        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private IActionResult RedirectToCartIndex()
        {
            return RedirectToAction("Index");
        }

        private void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        #endregion
    }
}