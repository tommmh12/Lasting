using Lasting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Lasting.Data;

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
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Tạo HTML đơn giản (có thể dùng Razor để render chuẩn hơn)
            var html = $@"
                <h2>Hóa đơn đơn hàng #{order.Id}</h2>
                <p>Ngày đặt: {order.OrderDate.ToLocalTime():dd/MM/yyyy HH:mm}</p>
                <p>Khách hàng: {order.ShippingAddress.FullName}</p>
                <hr />
                <table border='1' cellspacing='0' cellpadding='5' width='100%'>
                    <tr><th>Sản phẩm</th><th>Giá</th><th>Số lượng</th><th>Thành tiền</th></tr>
                    {string.Join("", order.OrderItems.Select(item => $"<tr><td>{item.Product?.Name}</td><td>{item.Price:N0}</td><td>{item.Quantity}</td><td>{item.Price * item.Quantity:N0}</td></tr>"))}
                    <tr><td colspan='3'><strong>Tổng cộng:</strong></td><td><strong>{order.TotalAmount:N0} đ</strong></td></tr>
                </table>";

                var pdfBytes = _pdfService.GeneratePdfFromHtml(html);

                return File(pdfBytes, "application/pdf", $"invoice_{order.Id}.pdf");
        }
    }
}
