using eCommerceApp.Application.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceApp.MVC.Controllers
{
    // Admin alanýna ait olmayan ana controller
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Öne çýkan ürünler için ilk 4 ürünü çekiyoruz
            // Normalde bu, IsFeatured bayraðý olan ürünler olmalýdýr.
            var featuredProducts = (await _productService.GetAllProductsAsync()).Take(4).ToList();

            // View Component'e göndermek için CategoryService'e gerek yok, 
            // View Component zaten kendi servisini kullanacak.

            return View(featuredProducts);
        }

        // Home Controller'a ait diðer basit action'lar (About, Privacy, vb.)
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
