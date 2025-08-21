using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;

namespace Zaptech.Controllers
{
    [RoutePrefix("Admin/Analytics")]
    [AdminLayout]
    [AdminAuthorization]
    public class AdminAnalyticsController : Controller
    {
        private DB_Conn db = new DB_Conn();

        [Route("BestSellingProducts")]
        public ActionResult BestSellingProducts(int? days)
        {
            int period = days ?? 30;
            DateTime fromDate = DateTime.Now.AddDays(-period);

            var data = db.OrderItems
                .Where(oi => oi.Order.Order_date >= fromDate && oi.Product != null) 
                .Include("Product.Category")                                         
                .ToList()                                                           
                .GroupBy(oi => oi.Product)
                .Where(g => g.Key != null)                                         
                .Select(g => new BestSellingProductVM
                {
                    Product = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = (decimal)g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToList();

            ViewBag.Days = period;
            return View(data);
        }



        [Route("TopCustomers")]
        public ActionResult TopCustomers(int? days)
        {
            int period = days ?? 30;
            DateTime fromDate = DateTime.Now.AddDays(-period);

            var data = db.Orders
                .Where(o => o.Order_date >= fromDate && o.User != null)
                .ToList()
                .GroupBy(o => o.User)
                .Select(g => new TopCustomerVM
                {
                    Customer = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = (decimal)g.Sum(o => o.Total_ammount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .ToList();

            ViewBag.Days = period;
            return View(data);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}