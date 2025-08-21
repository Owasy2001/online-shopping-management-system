using System.ComponentModel.DataAnnotations;
namespace Zaptech.Models
{
    public class SalesReportViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public float TotalRevenue { get; set; }

       
        public string RevenueFormatted => TotalRevenue.ToString("C2");
    }
}