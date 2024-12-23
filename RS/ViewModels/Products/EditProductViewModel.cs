using System.ComponentModel.DataAnnotations;

 namespace RS.ViewModels.Products
{
    public class EditProductViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Фото")]
        public string? ExistingPhoto { get; set; }

        public IFormFile? PhotoFile { get; set; }
        [Display(Name = "Роль")]
        public string RoleId { get; set; } // Добавляем RoleId для передачи текущей роли
    }
}