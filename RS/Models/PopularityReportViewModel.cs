using System.Collections.Generic;

namespace RS.Models
{
    public class PopularityReportViewModel
    {
        public List<ProductPopularityViewModel> ProductPopularity { get; set; }
        public List<ServicePopularityViewModel> ServicePopularity { get; set; }
    }

    public class ProductPopularityViewModel
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class ServicePopularityViewModel
    {
        public string ServiceName { get; set; }
        public int TotalUsage { get; set; }
    }
}
