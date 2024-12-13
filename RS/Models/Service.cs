using System.ComponentModel.DataAnnotations;

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

        public bool IsActive { get; set; } = true;

        public string? Photo { get; set; } // Path or URL to the service's image
    }
}
