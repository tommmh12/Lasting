using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Product
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedDate)
                .ToList(); 
            return View(products);
        }

        // GET: Admin/Product/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Hiện lỗi chi tiết nếu có
            AddModelErrorsToConsole();

            LoadDropdowns();
            return View(product);
        }
        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Product/Edit/5
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            LoadDropdowns();
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    // Lấy lại ảnh cũ nếu không upload ảnh mới
                    var existing = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                    if (existing != null)
                        product.ImageUrl = existing.ImageUrl;
                }

                _context.Products.Update(product);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(product);
        }


        // Dùng chung cho Create/Edit để hiển thị dropdown
        private void LoadDropdowns()
        {
            ViewBag.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Brands = _context.Brands.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();
        }

        // In lỗi ra console và hiển thị lên giao diện
        private void AddModelErrorsToConsole()
        {
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"Error in {modelState.Key}: {error.ErrorMessage}");
                    ModelState.AddModelError("", $"Lỗi tại '{modelState.Key}': {error.ErrorMessage}");
                }
            }
        }
        // GET: Admin/Product/Details/5
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        public IActionResult ToggleFeatured(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            product.IsFeatured = !product.IsFeatured;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            product.IsActive = !product.IsActive;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
