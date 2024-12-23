namespace RS.ViewModels.Orders
{
    public class ConfirmDeleteOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; } // Добавлено
        public string OrderStatus { get; set; }
    }
}
