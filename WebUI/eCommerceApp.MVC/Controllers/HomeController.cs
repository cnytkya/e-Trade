using eCommerceApp.Application.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceApp.MVC.Controllers
{
    // Admin alan�na ait olmayan ana controller
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
            // �ne ��kan �r�nler i�in ilk 4 �r�n� �ekiyoruz
            // Normalde bu, IsFeatured bayra�� olan �r�nler olmal�d�r.
            var featuredProducts = (await _productService.GetAllProductsAsync()).Take(4).ToList();

            // View Component'e g�ndermek i�in CategoryService'e gerek yok, 
            // View Component zaten kendi servisini kullanacak.

            return View(featuredProducts);
        }

        // Home Controller'a ait di�er basit action'lar (About, Privacy, vb.)
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
