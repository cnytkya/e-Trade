using eCommerceApp.Application.Interface.Repositories.Products;
using eCommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eCommerceApp.Infrastructure.Persistence.Repositories.Products
{
    public class ProductRepo : Repository<Product>, IProductRepositories
    {
        public ProductRepo(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Belirli bir alt kategoriye ait aktif ürün sayısını asenkron olarak sayar.
        /// </summary>
        public async Task<int> CountProductsBySubcategoryIdAsync(Guid subcategoryId)
        {
            return await _dbSet.CountAsync(p => p.SubcategoryId == subcategoryId);
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithSubcategoryAsync()
        {
            //Ürünleri, ait oldukları alt kategorileri ile birlikte getir.
            // Eager Loading: Ürünleri, alt kategorileri ve alt kategorilerin bağlı olduğu ana kategorileriyle birlikte getir.
            return await _dbSet.Include(p => p.Subcategory)
                .ThenInclude(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithSubcategoryIdAsync(Guid id)
        {
            //Belirli bir ürünü, ait olduğu alt kategoriyle birlikte ID'ye göre getir.
            return await _dbSet.Include(p => p.Subcategory)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

       
    }
}
