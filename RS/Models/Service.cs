using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RS.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [MaxLength(5000)] // Новый текст для описания услуги
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Photo { get; set; } // Path or URL to the service's image

        [Required]
        public string RoleId { get; set; }

        [ForeignKey("RoleId")]
        public IdentityRole? Role { get; set; }
    }
}
