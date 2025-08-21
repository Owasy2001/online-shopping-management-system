
using System.Collections.Generic;

namespace Zaptech.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<Order> RecentOrders { get; set; }
        
    }
}