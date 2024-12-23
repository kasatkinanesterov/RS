using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RS.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [MaxLength(5000)] // Новый текст для описания товара
        public string? Description { get; set; }

        public string? Photo { get; set; } // Path or URL to the product's image

        [Required]
        [MaxLength(450)]
        public string RoleId { get; set; }

        [ForeignKey("RoleId")]
        public IdentityRole? Role { get; set; }
    }
}
