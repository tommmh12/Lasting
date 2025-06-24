using Lasting.Areas.Admin.Models;
using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? status, string? fromDate, string? toDate, string? orderId)
        {
            var query = _context.Orders
                .Include(o => o.ShippingAddress)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
            {
                query = query.Where(o => o.OrderDate >= from);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
            {
                query = query.Where(o => o.OrderDate <= to.AddDays(1));
            }

            if (!string.IsNullOrEmpty(orderId))
            {
                if (orderId.Contains(','))
                {
                    var ids = orderId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => id.Trim())
                        .Where(id => int.TryParse(id, out _))
                        .Select(int.Parse)
                        .ToList();
                    if (ids.Any())
                        query = query.Where(o => ids.Contains(o.Id));
                }
                else
                {
                    query = query.Where(o => o.Id.ToString().Contains(orderId));
                }
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.TotalCount = orders.Count;
            ViewBag.TotalRevenue = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount);


            return View(orders);
        }


        // Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        // Báo cáo doanh thu theo ngày
        [HttpGet]
        public async Task<IActionResult> Report(string? status, string? fromDate, string? toDate)
        {
            var query = _context.Orders
                .Include(o => o.ShippingAddress)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            DateTime? from = null;
            DateTime? to = null;

            if (DateTime.TryParse(fromDate, out var fromParsed))
            {
                from = fromParsed;
                query = query.Where(o => o.OrderDate >= fromParsed);
            }

            if (DateTime.TryParse(toDate, out var toParsed))
            {
                to = toParsed;
                query = query.Where(o => o.OrderDate <= toParsed.AddDays(1));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // Thống kê doanh thu theo ngày
            var dailyStats = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueViewModel
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();
            var model = new ReportViewModel
            {
                Orders = orders,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalCount = orders.Count,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                DailyStats = dailyStats
            };

            return View(model);
        }
    }
}
