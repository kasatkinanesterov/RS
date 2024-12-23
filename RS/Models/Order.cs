using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RS.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } // Ссылаемся на IdentityUser

        [ForeignKey("EmployeeId")]
        public IdentityUser? Employee { get; set; } // Используем IdentityUser вместо Employee

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Phone]
        public string CustomerPhone { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
