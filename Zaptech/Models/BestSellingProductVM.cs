using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
    public class BestSellingProductVM
    {
        public Product Product { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCustomerVM
    {
        public User Customer { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}