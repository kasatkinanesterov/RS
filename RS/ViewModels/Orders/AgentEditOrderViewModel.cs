using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.Orders
{
    public class AgentEditOrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя клиента обязательно")]
        [MaxLength(100)]
        public string CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string CustomerEmail { get; set; }

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Статус заказа обязателен")]
        [MaxLength(50)]
        public string OrderStatus { get; set; }

        public static List<string> AvailableStatuses => new List<string>
        {
            "СОЗДАНО",
            "НА ИСПОЛНЕНИИ",
            "ПРИОСТАНОВЛЕНО",
            "ЗАВЕРШЕНО",
            "ОТМЕНЕНО"
        };
    }
}
