using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.OrderServices
{
    public class OrderServiceEditViewModel
    {
        public int Id { get; set; } // ID записи OrderService (при редактировании)

        [Required]
        public int OrderId { get; set; } // ID заказа

        [Required]
        public int ServiceId { get; set; } // ID услуги

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
        public DateTime Deadline { get; set; }

        [Display(Name = "Исполнитель")]
        public string? ExecutorId { get; set; } // Исполнитель услуги
    }
}
