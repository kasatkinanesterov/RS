using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.Products
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [MaxLength(5000)]
        public string? Description { get; set; }

        public string? Photo { get; set; } // Ссылка на текущее фото

        public string? ExistingPhoto { get; set; } // Для хранения ссылки на существующее фото при редактировании

        [Required]
        public string RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
