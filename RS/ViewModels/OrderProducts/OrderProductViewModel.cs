using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.OrderProducts
{
    public class OrderProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }

        public string? Status { get; set; }
        public string? ExecutorName { get; set; }
        public string? AdditionalInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime Deadline { get; set; }

        public string? ExecutorId { get; set; }

    }
}
