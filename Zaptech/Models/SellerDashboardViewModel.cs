using System.Collections.Generic;

namespace Zaptech.Models
{
    public class SellerDashboardViewModel
    {
        public User Seller { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int PendingOrders { get; set; } 
        public List<OrderItem> RecentOrders { get; set; }
    }
}