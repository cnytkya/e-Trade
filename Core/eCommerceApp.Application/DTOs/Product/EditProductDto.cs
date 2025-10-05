using System.ComponentModel.DataAnnotations;

namespace eCommerceApp.Application.DTOs.Product
{
    public class EditProductDto
    {
        [Required(ErrorMessage = "Ürün ID'si boş bırakılamaz.")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Ürün adı boş bırakılamaz")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla {1} karakter olabilir.")]
        public string Name { get; set; }
        [Display(Name = "Ürün Açıklaması")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat alanı boş bırakılamaz.")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat negatif olamaz!")]
        [Display(Name = "Fiyat (₺)")] //Alt Gr + T
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok bilgisi boş bırakılamaz.")]
        [Display(Name = "Ürün Adedi")]
        public int Stock { get; set; }

        [Display(Name = "Stok kodu")]
        [StringLength(50, ErrorMessage = "Stok kodu en fazla {1} karakter olmalı.")]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Alt kategori seçimi zorunlu.")]
        public Guid SubcategoryId { get; set; }
        // YENİ EKLENEN ÖZELLİK: Mevcut resim URL'si
        [Display(Name = "Resim URL'si")]
        public string? ImageUrl { get; set; }
    }
}
