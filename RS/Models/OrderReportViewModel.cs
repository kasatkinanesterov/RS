using System;
using System.Collections.Generic;

namespace RS.Models
{
    public class OrderReportViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string EmployeeName { get; set; }
        public List<ProductReportItem> Products { get; set; } = new List<ProductReportItem>();
        public List<string> Services { get; set; } = new List<string>();
    }

    public class ProductReportItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
