using System.Security.Claims;
using Lasting.Data;
using Lasting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lasting.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            productsQuery = productsQuery.OrderByDescending(p => p.CreatedDate);

            var products = await productsQuery.ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;

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
                .Include(p => p.Reviews).ThenInclude(r => r.Replies)
                .Include(p => p.Gallery)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.CategoryName = product.Category?.Name;
            ViewBag.BrandName = product.Brand?.Name;
            ViewBag.AverageRating = product.Reviews?.Any() == true
                ? product.Reviews.Average(r => r.Rating)
                : 0.0;

            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, string content, int rating, List<IFormFile> images)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(content) || rating < 1 || rating > 5 || userId == null)
                return RedirectToAction("Details", new { id = productId });

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Content = content,
                Rating = rating,
                CreatedAt = DateTime.UtcNow,
                Images = new List<ReviewImage>()
            };

            if (images != null && images.Any())
            {
                foreach (var image in images.Take(3)) // Giới hạn 3 ảnh
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var path = Path.Combine("wwwroot/images/reviews", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    using (var stream = new FileStream(path, FileMode.Create))
                        await image.CopyToAsync(stream);

                    review.Images.Add(new ReviewImage
                    {
                        ImageUrl = "/images/reviews/" + fileName
                    });
                }
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = productId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyReview(int reviewId, string replyContent)
        {
            if (string.IsNullOrWhiteSpace(replyContent))
                return RedirectToAction("Details", new { id = 0 });

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            var reply = new ReviewReply
            {
                ReviewId = reviewId,
                Content = replyContent,
                AdminId = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };

            _context.ReviewReplies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = review.ProductId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null) return NotFound();

            _context.ReviewImages.RemoveRange(review.Images);
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = review.ProductId });
        }
    }
}
