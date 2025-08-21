using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
    // Models/ViewModels/EarningsViewModel.cs
    public class EarningsViewModel
    {
        public decimal TotalEarnings { get; set; }
        public int TotalOrders { get; set; }
        public List<DailyEarning> DailyEarnings { get; set; }
        public List<MonthlyEarning> MonthlyEarnings { get; set; }
    }

    public class DailyEarning
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public class MonthlyEarning
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }

        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
    }
}