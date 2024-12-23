using RS.ViewModels.OrderProducts;
using RS.ViewModels.OrderServices;

namespace RS.ViewModels.Orders
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string OrderStatus { get; set; }
        public string EmployeeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderProductViewModel> Products { get; set; } = new List<OrderProductViewModel>();
        public List<OrderServiceViewModel> Services { get; set; } = new List<OrderServiceViewModel>();

    }
}
