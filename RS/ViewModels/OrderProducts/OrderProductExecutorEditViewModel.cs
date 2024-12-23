using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.OrderProducts
{
    public class OrderProductExecutorEditViewModel
    {
        public int Id { get; set; } // ID товара в заказе
        public string Status { get; set; } // Статус товара
        public static List<string> AvailableStatuses => new List<string>
        {
            "СОЗДАНО",
            "НА ИСПОЛНЕНИИ",
            "ПРИОСТАНОВЛЕНО",
            "ЗАВЕРШЕНО",
            "ОТМЕНЕНО"
        };
        public string AdditionalInfo { get; set; } // Дополнительная информация
    }
}
