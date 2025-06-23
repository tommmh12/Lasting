using Lasting.Models;
using Lasting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lasting.Controllers
{
    // Controllers/CheckoutController.cs
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var cartTotal = await _cartService.GetCartTotalAsync(userId);

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartTotal,
                ShippingAddress = new ShippingAddress()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                model.CartItems = await _cartService.GetCartItemsAsync(userId);
                model.TotalAmount = await _cartService.GetCartTotalAsync(userId);
                return View("Index", model);
            }

            var order = await _orderService.CreateOrderAsync(userId, model.ShippingAddress);

            if (order != null)
            {
                await _cartService.ClearCartAsync(userId);
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            ModelState.AddModelError("", "Có lỗi xảy ra khi đặt hàng");
            return View("Index", model);
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            return View(orderId);
        }
    }
}
