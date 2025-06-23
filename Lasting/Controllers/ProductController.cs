using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search = null, int categoryId = 0)
        {
            var productsQuery = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery
                    .Where(p => p.Name.Contains(search));
            }

            if (categoryId > 0)
            {
                productsQuery = productsQuery
                    .Where(p => p.CategoryId == categoryId);
            }

            productsQuery = productsQuery
                .OrderByDescending(p => p.CreatedDate);

            var products = await productsQuery.ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;

            return View(products);
        }
    }
}
