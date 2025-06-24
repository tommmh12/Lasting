// OrdersController.cs
using Lasting.Data;
using Lasting.Models;
using Lasting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Lasting.Controllers
{
    [Authorize(Policy = "RequireCustomer")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public OrdersController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();

            if (order.Status != OrderStatus.Delivered)
            {
                TempData["Error"] = "Chỉ có thể xuất hóa đơn khi đơn hàng đã giao.";
                return RedirectToAction("Details", new { id });
            }

            var html = $@"<h2>Hóa đơn #{order.Id}</h2>
                <p>Ngày: {order.OrderDate:dd/MM/yyyy}</p>
                <p>Khách hàng: {order.ShippingAddress?.FullName}</p>
                <table border='1' cellspacing='0' cellpadding='5'>
                    <tr><th>Sản phẩm</th><th>Giá</th><th>SL</th><th>Thành tiền</th></tr>
                    {string.Join("", order.OrderItems.Select(i => $"<tr><td>{i.Product?.Name}</td><td>{i.Price:N0}</td><td>{i.Quantity}</td><td>{i.Price * i.Quantity:N0}</td></tr>"))}
                    <tr><td colspan='3'>Tổng cộng</td><td><strong>{order.TotalAmount:N0} đ</strong></td></tr>
                </table>";

            var pdf = _pdfService.GeneratePdfFromHtml(html);
            return File(pdf, "application/pdf", $"invoice_{order.Id}.pdf");
        }

        // Gọi trong lúc xác nhận đơn hàng
        public async Task<bool> UpdateInventoryAfterOrderAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    if (item.Product.StockQuantity <= 0)
                    {
                        item.Product.StockQuantity = 0;
                        item.Product.IsActive = false;
                    }
                    _context.Products.Update(item.Product);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string CancelReason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null ||
                (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing))
            {
                TempData["Error"] = "Đơn hàng không thể hủy.";
                return RedirectToAction("Details", new { id });
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelReason = CancelReason;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            return RedirectToAction("MyOrders");
        }

    }
}
