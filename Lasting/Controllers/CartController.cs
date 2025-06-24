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
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || !product.IsActive)
            {
                SetErrorMessage("Sản phẩm hiện không khả dụng.");
                return RedirectToCartIndex();
            }

            var userId = GetCurrentUserId();
            int currentQuantity = 0;

            if (User.Identity.IsAuthenticated)
            {
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                currentQuantity = cartItems.Where(c => c.ProductId == productId).Sum(c => c.Quantity);
            }
            else
            {
                var tempCart = await _cartService.GetTempCartAsync();
                currentQuantity = tempCart.Where(x => x.ProductId == productId).Sum(x => x.Quantity);
            }

            if (currentQuantity + quantity > product.StockQuantity)
            {
                SetErrorMessage($"Số lượng vượt quá tồn kho ({product.StockQuantity} sản phẩm).");
                return RedirectToCartIndex();
            }

            await _cartService.AddToCartAsync(userId, productId, quantity);
            SetSuccessMessage($"Đã thêm {product.Name} vào giỏ hàng");
            return RedirectToCartIndex();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id, bool isTempItem = false)
        {
            if (User.Identity.IsAuthenticated && !isTempItem)
                await _cartService.RemoveFromCartAsync(GetCurrentUserId(), id);
            else
                await _cartService.RemoveTempCartItemAsync(id);

            SetSuccessMessage("Đã xóa sản phẩm khỏi giỏ hàng");
            return RedirectToCartIndex();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTempCartItem([FromBody] UpdateTempCartRequest request)
        {
            var product = await _productService.GetProductByIdAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest("Sản phẩm không tồn tại.");
            }

            var tempCart = await _cartService.GetTempCartAsync();
            var currentItem = tempCart.FirstOrDefault(x => x.ProductId == request.ProductId);
            int currentQuantity = currentItem?.Quantity ?? 0;

            if (request.Quantity <= 0)
            {
                await _cartService.RemoveTempCartItemAsync(request.ProductId);
            }
            else if (request.Quantity > product.StockQuantity)
            {
                return BadRequest($"Số lượng vượt quá tồn kho ({product.StockQuantity} sản phẩm).");
            }
            else
            {
                await _cartService.UpdateTempCartItemAsync(request.ProductId, request.Quantity);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTempCartItem([FromBody] RemoveTempCartRequest request)
        {
            await _cartService.RemoveTempCartItemAsync(request.ProductId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int newQuantity, bool isTempItem = false)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                SetErrorMessage("Sản phẩm không tồn tại.");
                return RedirectToCartIndex();
            }

            int currentQuantity = 0;
            if (User.Identity.IsAuthenticated && !isTempItem)
            {
                var cartItems = await _cartService.GetCartItemsAsync(GetCurrentUserId());
                currentQuantity = cartItems.Where(c => c.ProductId == id).Sum(c => c.Quantity);
            }
            else
            {
                var tempCart = await _cartService.GetTempCartAsync();
                currentQuantity = tempCart.Where(x => x.ProductId == id).Sum(x => x.Quantity);
            }

            if (newQuantity <= 0)
            {
                await RemoveFromCart(id, isTempItem);
                return RedirectToCartIndex();
            }
            else if (newQuantity > product.StockQuantity)
            {
                SetErrorMessage($"Số lượng vượt quá tồn kho ({product.StockQuantity} sản phẩm).");
                return RedirectToCartIndex();
            }

            await ProcessUpdateQuantity(id, newQuantity, isTempItem);
            return RedirectToCartIndex();
        }

        public class UpdateTempCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class RemoveTempCartRequest
        {
            public int ProductId { get; set; }
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
                return (await _cartService.GetCartItemsAsync(GetCurrentUserId()), "Index");
            return (await _cartService.GetTempCartAsync(), "TempCart");
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

        private void SetErrorMessage(string message) => TempData["ErrorMessage"] = message;
        #endregion
    }
}