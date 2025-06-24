using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanGiay.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WebsiteBanGiay.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Description,Price,StockQuantity,CategoryId,BrandId,IsActive,ImageFiles")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh đại diện
                if (product.ImageFiles != null && product.ImageFiles.Count > 0)
                {
                    var firstImage = product.ImageFiles[0];
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + firstImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await firstImage.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                product.CreatedAt = DateTime.Now;
                _dbContext.Add(product);
                await _dbContext.SaveChangesAsync();

                // Xử lý upload nhiều ảnh sản phẩm
                if (product.ImageFiles != null && product.ImageFiles.Count > 1)
                {
                    for (int i = 1; i < product.ImageFiles.Count; i++)
                    {
                        var imageFile = product.ImageFiles[i];
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        var productImage = new ProductImage
                        {
                            Url = "/images/products/" + uniqueFileName,
                            ProductId = product.ProductId
                        };

                        _dbContext.Add(productImage);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _dbContext.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Description,Price,StockQuantity,CategoryId,BrandId,IsActive,ImageUrl,ImageFiles")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh đại diện mới nếu có
                    if (product.ImageFiles != null && product.ImageFiles.Count > 0)
                    {
                        var firstImage = product.ImageFiles[0];
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + firstImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await firstImage.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    // Xử lý upload nhiều ảnh sản phẩm mới
                    if (product.ImageFiles != null && product.ImageFiles.Count > 1)
                    {
                        for (int i = 1; i < product.ImageFiles.Count; i++)
                        {
                            var imageFile = product.ImageFiles[i];
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            var productImage = new ProductImage
                            {
                                Url = "/images/products/" + uniqueFileName,
                                ProductId = product.ProductId
                            };

                            _dbContext.Add(productImage);
                        }
                    }

                    _dbContext.Update(product);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _dbContext.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // Xóa ảnh đại diện
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa các ảnh sản phẩm
                foreach (var image in product.Images)
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.Url.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _dbContext.Products.Any(e => e.ProductId == id);
        }
    }
}