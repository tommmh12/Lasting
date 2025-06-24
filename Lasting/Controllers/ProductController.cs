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
                .Where(p => p.IsActive)
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CategoryName = product.Category?.Name;
            ViewBag.BrandName = product.Brand?.Name;

            return View(product);
        }

        public async Task<IActionResult> NewProducts()
        {
            // Lấy 3 sản phẩm mới nhất
            var newProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToListAsync();

            if (newProducts == null || !newProducts.Any())
            {
                return NotFound("Không có sản phẩm mới.");
            }

            return View(newProducts);
        }
        public async Task<IActionResult> MaleProducts()
        {
            var maleProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Category != null && p.Category.Name == "Nam")
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            if (!maleProducts.Any()) // Chỉ kiểm tra .Any() vì maleProducts không null
            {
                return NotFound("Không có sản phẩm cho nam.");
            }

            return View("ProductsByGender", maleProducts);
        }

        public async Task<IActionResult> FemaleProducts()
        {
            var femaleProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Category != null && p.Category.Name == "Nữ")
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            if (!femaleProducts.Any())
            {
                return NotFound("Không có sản phẩm cho nữ.");
            }

            return View("ProductsByGender", femaleProducts);
        }
    }
}