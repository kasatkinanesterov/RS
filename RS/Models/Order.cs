using RS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; }

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

    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
