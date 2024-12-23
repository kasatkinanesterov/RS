using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.OrderServices
{
    public class OrderServiceExecutorEditViewModel
    {
        public int Id { get; set; } // ID услуги в заказе

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Статус услуги
        public static List<string> AvailableStatuses => new List<string>
        {
            "СОЗДАНО",
            "НА ИСПОЛНЕНИИ",
            "ПРИОСТАНОВЛЕНО",
            "ЗАВЕРШЕНО",
            "ОТМЕНЕНО"
        };
        public string? AdditionalInfo { get; set; } // Дополнительная информация
    }
}
