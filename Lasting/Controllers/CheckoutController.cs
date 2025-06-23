using Lasting.Models;
using Lasting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lasting.Controllers
{
    [Authorize(Policy = "RequireCustomer")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;

        public CheckoutController(ICartService cartService, IOrderService orderService, IEmailService emailService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItems = await _cartService.GetCartItemsAsync(userId);
            var total = await _cartService.GetCartTotalAsync(userId);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = total,
                ShippingAddress = new ShippingAddress()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine(">>>>> UserId: " + userId);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            model.CartItems = await _cartService.GetCartItemsAsync(userId);
            model.TotalAmount = await _cartService.GetCartTotalAsync(userId);

            if (model.CartItems == null || !model.CartItems.Any())
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"[ModelError] {kvp.Key}: {error.ErrorMessage}");
                    }
                }

                return View(model);
            }

            var order = await _orderService.CreateOrderAsync(userId, model.ShippingAddress);

            if (order == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng. Vui lòng thử lại.");
                return View(model);
            }

            await _cartService.ClearCartAsync(userId);

            // Gửi email xác nhận
            await _emailService.SendEmailAsync(
                toEmail: User.Identity.Name, // hoặc lấy email từ DB nếu muốn chính xác hơn
                subject: $"Xác nhận đơn hàng #{order.Id}",
                htmlMessage: $"<p>Xin chào,</p><p>Đơn hàng <strong>#{order.Id}</strong> đã được ghi nhận thành công.</p><p>Tổng cộng: <strong>{order.TotalAmount:N0} đ</strong></p><p>Trân trọng,<br>OTI Team</p>"
            );

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _orderService.GetOrderDetailsAsync(orderId);

            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
