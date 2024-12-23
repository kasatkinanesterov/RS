using System;

namespace RS.ViewModels.OrderServices
{
    public class OrderServiceViewModel
    {
        public int Id { get; set; } // ID услуги в заказе

        public string ServiceName { get; set; } // Название услуги
        public decimal ServicePrice { get; set; } // Цена услуги

        public string? Status { get; set; } // Статус услуги
        public string? ExecutorName { get; set; } // Имя исполнителя услуги
        public string? ExecutorId { get; set; } // ID исполнителя

        public string? AdditionalInfo { get; set; } // Дополнительная информация

        public DateTime CreatedAt { get; set; } // Дата создания услуги в заказе
        public DateTime UpdatedAt { get; set; } // Дата последнего изменения услуги
        public DateTime Deadline { get; set; }
    }
}
