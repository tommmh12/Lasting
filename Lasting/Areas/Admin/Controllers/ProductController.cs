using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();
            return View(products);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .Include(p => p.Reviews).ThenInclude(r => r.Images)
                .Include(p => p.Gallery)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.CategoryName = product.Category?.Name;
            ViewBag.BrandName = product.Brand?.Name;

            return View(product);
        }

        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                UploadImage(product, imageFile);
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            AddModelErrorsToConsole();
            LoadDropdowns();
            return View(product);
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            LoadDropdowns();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                UploadImage(product, imageFile, isEdit: true);
                _context.Products.Update(product);
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
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

        private void UploadImage(Product product, IFormFile imageFile, bool isEdit = false)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                if (isEdit)
                {
                    var existing = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                    if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        var oldPath = Path.Combine("wwwroot", existing.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                }

                product.ImageUrl = "/images/products/" + uniqueFileName;
            }
            else if (isEdit)
            {
                var existing = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == product.Id);
                if (existing != null) product.ImageUrl = existing.ImageUrl;
            }
        }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyReview(int reviewId, string replyContent)
        {
            if (string.IsNullOrWhiteSpace(replyContent)) return RedirectToAction("Details", new { id = reviewId });

            var review = await _context.Reviews.Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null) return NotFound();

            review.AdminReply = replyContent;
            review.RepliedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = review.ProductId });
        }
    }
}
