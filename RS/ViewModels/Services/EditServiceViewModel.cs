using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.Services
{
    public class EditServiceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Фото")]
        public string? ExistingPhoto { get; set; }

        public IFormFile? PhotoFile { get; set; }

        [Required]
        public string RoleId { get; set; } // Передаем для скрытого поля

        [Display(Name = "Активна")]
        public bool IsActive { get; set; }
    }
}
