using AutoMapper;
using eCommerceApp.Application.DTOs.Product;
using eCommerceApp.Application.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eCommerceApp.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMapper _mapper;

        public ProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment hostEnvironment, IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
        }

        // Alt kategorileri dropdown için hazırla
        private async Task PrepareSubcategoryViewBag(Guid? selectedId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var subcategoryList = new List<SelectListItem>();

            foreach (var category in categories)
            {
                if (category.SubCategories != null && category.SubCategories.Any())
                {
                    var categoryGroup = new SelectListGroup { Name = category.Name };

                    foreach (var subcategory in category.SubCategories)
                    {
                        subcategoryList.Add(new SelectListItem
                        {
                            Text = subcategory.Name,
                            Value = subcategory.Id.ToString(),
                            Group = categoryGroup,
                            Selected = (selectedId.HasValue && subcategory.Id == selectedId.Value)
                        });
                    }
                }
            }

            // Direkt listeyi gönderiyoruz
            ViewBag.Subcategories = subcategoryList;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Yeni Ürün Oluştur";
            await PrepareSubcategoryViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto model, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Ürün resmi seçilmesi zorunludur.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doğru doldurun.";
                await PrepareSubcategoryViewBag(model.SubcategoryId);
                return View(model);
            }

            string uniqueFileName = null;
            string wwwRootPath = _hostEnvironment.WebRootPath;

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(wwwRootPath, "img", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string extension = Path.GetExtension(imageFile.FileName);
                    uniqueFileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = Path.Combine("~/", "img", "products", uniqueFileName).Replace("\\", "/");
                }

                var (succeeded, errors) = await _productService.CreateProductAsync(model, imageFile);

                if (succeeded)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrEmpty(uniqueFileName))
                {
                    string fileToDeletePath = Path.Combine(wwwRootPath, "img", "products", uniqueFileName);
                    if (System.IO.File.Exists(fileToDeletePath))
                    {
                        System.IO.File.Delete(fileToDeletePath);
                    }
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Dosya yüklenirken beklenmeyen bir hata oluştu: " + ex.Message);
            }

            await PrepareSubcategoryViewBag(model.SubcategoryId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var productDto = await _productService.GetProductByIdAsync(id);
            if (productDto == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = _mapper.Map<EditProductDto>(productDto);
            await PrepareSubcategoryViewBag(model.SubcategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProductDto model, IFormFile? imageFile)
        {
            string uniqueFileName = null;
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string oldImagePath = model.ImageUrl;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doğru doldurun.";
                await PrepareSubcategoryViewBag(model.SubcategoryId);
                return View(model);
            }

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(wwwRootPath, "img", "products");
                    string extension = Path.GetExtension(imageFile.FileName);
                    uniqueFileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = Path.Combine("~/", "img", "products", uniqueFileName).Replace("\\", "/");

                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        string oldFileName = Path.GetFileName(oldImagePath);
                        string fileToDeletePath = Path.Combine(wwwRootPath, "img", "products", oldFileName);

                        if (System.IO.File.Exists(fileToDeletePath))
                        {
                            System.IO.File.Delete(fileToDeletePath);
                        }
                    }
                }

                var (succeeded, errors) = await _productService.UpdateProductAsync(model);

                if (succeeded)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrEmpty(uniqueFileName))
                {
                    string fileToDeletePath = Path.Combine(wwwRootPath, "img", "products", uniqueFileName);
                    if (System.IO.File.Exists(fileToDeletePath))
                    {
                        System.IO.File.Delete(fileToDeletePath);
                    }
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Dosya yüklenirken beklenmeyen bir hata oluştu: " + ex.Message);
            }

            await PrepareSubcategoryViewBag(model.SubcategoryId);
            return View(model);
        }

        // 1. ÜRÜN SİLME (DELETE) - GET (ONAY SAYFASI)
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Sayfa başlığı bilgisi View’a aktarılıyor.
            ViewData["Title"] = "Ürün Silme Onayı";

            // Silinmek istenen ürün, id’ye göre servis üzerinden getiriliyor.
            var productDto = await _productService.GetProductByIdAsync(id);

            // Eğer ürün bulunmazsa, hata mesajı gönder ve Index sayfasına yönlendir.
            if (productDto == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Ürün bulunduysa, silme onay sayfasına productDto bilgisiyle git.
            return View(productDto);
        }


        // 2. ÜRÜN SİLME (DELETE) - POST (GERÇEK SİLME)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Silinmeden önce, ürünün bilgilerini alıyoruz (özellikle resmi varsa path için).
            var productDto = await _productService.GetProductByIdAsync(id);

            // Servis üzerinden ürünü veritabanından silme işlemi yapılıyor.
            var (succeeded, errors) = await _productService.DeleteProductByIdAsync(id);

            if (succeeded)
            {
                // Eğer silme başarılı olduysa, ürünün resmi fiziksel dosya sisteminden de siliniyor.
                if (!string.IsNullOrEmpty(productDto?.ImageUrl))
                {
                    // wwwroot dizininin fiziksel yolu alınıyor.
                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    // Eski dosya yolu oluşturuluyor (ImageUrl'deki ~ ve / düzeltiliyor).
                    string oldFilePath = Path.Combine(
                        wwwRootPath,
                        productDto.ImageUrl.TrimStart('~', '/').Replace("/", "\\")
                    );

                    // Eğer dosya gerçekten varsa, fiziksel olarak siliniyor.
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Başarı mesajı TempData ile Index sayfasında gösterilmek üzere ayarlanıyor.
                TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
            }
            else
            {
                // Eğer silme başarısızsa, gelen hata mesajları TempData’ya yazılıyor.
                TempData["ErrorMessage"] =
                    "Ürün silinirken bir hata oluştu: " + string.Join(", ", errors);
            }

            // Silme işleminden sonra tekrar Index sayfasına yönlendiriliyor.
            return RedirectToAction(nameof(Index));
        }

    }
}
