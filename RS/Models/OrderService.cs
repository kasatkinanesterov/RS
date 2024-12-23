using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RS.Models
{
    public class OrderService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        public string? ExecutorId { get; set; }

        [ForeignKey("ExecutorId")]
        public IdentityUser? Executor { get; set; }

        public string? AdditionalInfo { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Новый статус товара в заказе

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; }
    }
}
