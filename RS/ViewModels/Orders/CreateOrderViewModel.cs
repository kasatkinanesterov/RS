using Microsoft.AspNetCore.Mvc.Rendering;

namespace RS.ViewModels.Orders
{
    public class CreateOrderViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string OrderStatus { get; set; }
        public static List<string> AvailableStatuses => new List<string>
        {
            "СОЗДАНО",
            "НА ИСПОЛНЕНИИ",
            "ПРИОСТАНОВЛЕНО",
            "ЗАВЕРШЕНО",
            "ОТМЕНЕНО"
        };
        public string EmployeeId { get; set; }
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    }
}
