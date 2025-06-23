using Lasting.Models;
using Lasting.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lasting.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProductService productService, ILogger<HomeController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            return View(featuredProducts);
        }

        public async Task<IActionResult> Products(int? categoryId, int? brandId)
        {
            IEnumerable<Product> products;

            if (categoryId.HasValue)
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            else if (brandId.HasValue)
                products = await _productService.GetProductsByBrandAsync(brandId.Value);
            else
                products = await _productService.GetAllProductsAsync();

            return View(products);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var allCategoryProducts = await _productService.GetProductsByCategoryAsync(product.CategoryId);

            ViewBag.RelatedProducts = allCategoryProducts
                .Where(p => p.Id != id)
                .Take(4)
                .ToList();

            return View(product);
        }
    }
}