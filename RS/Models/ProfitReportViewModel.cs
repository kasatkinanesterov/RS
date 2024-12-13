using System.Collections.Generic;

namespace RS.Models
{
    public class ProfitReportViewModel
    {
        public List<ProfitProductViewModel> ProductProfits { get; set; }
        public List<ProfitServiceViewModel> ServiceProfits { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class ProfitProductViewModel
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class ProfitServiceViewModel
    {
        public string ServiceName { get; set; }
        public int TotalUsage { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
