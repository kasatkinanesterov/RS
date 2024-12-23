using System.ComponentModel.DataAnnotations;

namespace RS.ViewModels.OrderProducts
{
    public class OrderProductEditViewModel
    {
        public int Id { get; set; } // ID записи OrderProduct (при редактировании)

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        public static List<string> AvailableStatuses => new List<string>
        {
            "СОЗДАНО",
            "НА ИСПОЛНЕНИИ",
            "ПРИОСТАНОВЛЕНО",
            "ЗАВЕРШЕНО",
            "ОТМЕНЕНО"
        };
        public string? AdditionalInfo { get; set; }
        public DateTime Deadline { get; set; }

        [Display(Name = "Исполнитель")]
        public string? ExecutorId { get; set; }

    }
}
