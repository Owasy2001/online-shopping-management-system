// AdminController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;
using PagedList;
using Rotativa;
using System.Collections.Generic;

namespace Zaptech.Controllers
{
    [AdminLayout]
    [AdminAuthorization]
    public class AdminController : Controller
    {
        private DB_Conn db = new DB_Conn();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            // Dashboard statistics
            var model = new AdminDashboardViewModel
            {
                TotalSales = db.Payments.Count(),
                TotalOrders = db.Orders.Count(),
                NewOrders = db.Orders.Count(o => o.Status == "Pending"),
                ProcessingOrders = db.Orders.Count(o => o.Status == "Processing"),
                CompletedOrders = db.Orders.Count(o => o.Status == "Completed"),
                CancelledOrders = db.Orders.Count(o => o.Status == "Cancelled"),
                RecentOrders = db.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.Order_date)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }


      
        public ActionResult Users(string searchString, string roleFilter, string statusFilter, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.StatusFilter = statusFilter;

            // Include Orders in the query
            IQueryable<User> users = db.Users.Include(u => u.Orders);

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Name.Contains(searchString) || u.Email.Contains(searchString));
            }

            // Role filter
            if (!string.IsNullOrEmpty(roleFilter))
            {
                var roleValue = int.Parse(roleFilter);
                users = users.Where(u => u.Role == (UserRole)roleValue);
            }

            // Status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "Active";
                users = users.Where(u => u.IsActive == isActive);
            }

            // Prepare role dropdown
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Where(r => r != UserRole.Admin)
                            .Select(r => new SelectListItem
                            {
                                Value = ((int)r).ToString(),
                                Text = r.ToString()
                            }).ToList();

            ViewBag.RoleList = new SelectList(roles, "Value", "Text");

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(users.OrderBy(u => u.Name).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult UserDetails(int id)
        {
            var user = db.Users
                .Include(u => u.Orders)
                .Include(u => u.Orders.Select(o => o.OrderItems))
                .Include(u => u.Orders.Select(o => o.OrderItems.Select(oi => oi.Product)))
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult ToggleUserStatus(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            TempData["Message"] = $"User {(user.IsActive ? "activated" : "blocked")} successfully";
            return RedirectToAction("Users");
        }


        

        // GET: Admin/Orders
        public ActionResult Orders(string searchString, string statusFilter, DateTime? fromDate, DateTime? toDate, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            IQueryable<Order> orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems);

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o =>
                    o.User.Name.Contains(searchString) ||
                    o.Id.ToString() == searchString ||
                    o.Payment.Transiction_Id.Contains(searchString));
            }

            // Status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter);
            }

            // Date range filter
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.Order_date >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.Order_date <= toDate.Value);
            }

            // Prepare status dropdown
            var statuses = new List<string> { "Pending", "Processing", "Completed", "Cancelled", "Refunded" };
            ViewBag.StatusList = new SelectList(statuses);

            int pageSize = 15;
            int pageNumber = (page ?? 1);

            return View(orders.OrderByDescending(o => o.Order_date).ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Validate status transition
            if (order.Status == "Completed" && status == "Cancelled")
            {
                return Json(new { success = false, message = "Cannot cancel a completed order" });
            }

            order.Status = status;
            db.SaveChanges();

            return Json(new { success = true, message = "Order status updated successfully" });
        }

        // GET: Admin/RefundRequests
        public ActionResult RefundRequests()
        {
            var refundRequests = db.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Where(o => o.Status == "Refund Requested")
                .OrderByDescending(o => o.Order_date)
                .ToList();

            return View(refundRequests);
        }

        // POST: Admin/ProcessRefund
        [HttpPost]
        public ActionResult ProcessRefund(int orderId, bool approve, string reason)
        {
            var order = db.Orders
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            if (approve)
            {
                order.Status = "Refunded";
                order.Payment.Status = "Refunded";

                // Restore product stock
                foreach (var item in db.OrderItems.Where(oi => oi.OrderId == orderId))
                {
                    var product = db.Products.Find(item.ProductId);
                    product.Stock += item.Quantity;
                }
            }
            else
            {
                order.Status = "Completed"; // Reject refund request
            }

            db.SaveChanges();

            // In a real app, you'd send email notification here
            TempData["Message"] = $"Refund request {(approve ? "approved" : "rejected")} successfully";
            return RedirectToAction("RefundRequests");
        }



        // GET: Admin/Payments
        public ActionResult Payments(string searchString, string statusFilter, DateTime? fromDate, DateTime? toDate, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            IQueryable<Payment> payments = db.Payments
                .Include(p => p.Order)
                .Include(p => p.Order.User);

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                payments = payments.Where(p =>
                    p.Transiction_Id.Contains(searchString) ||
                    p.Order.User.Name.Contains(searchString) ||
                    p.Order.Id.ToString() == searchString);
            }

            // Status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                payments = payments.Where(p => p.Status == statusFilter);
            }

            // Date range filter
            if (fromDate.HasValue)
            {
                payments = payments.Where(p => p.Paid_at >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                payments = payments.Where(p => p.Paid_at <= toDate.Value);
            }

            // Prepare status dropdown
            var statuses = new List<string> { "Paid", "Refunded", "Failed" };
            ViewBag.StatusList = new SelectList(statuses);

            int pageSize = 15;
            int pageNumber = (page ?? 1);

            return View(payments.OrderByDescending(p => p.Paid_at).ToPagedList(pageNumber, pageSize));
        }


        // GET: Admin/Earnings
        public ActionResult Earnings(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.GroupBy = groupBy;

            // Initial query with filter and eager loading
            IQueryable<Payment> paymentsQuery = db.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == "Paid" && p.Order != null);

            // Apply date filters
            if (fromDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Paid_at >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Paid_at <= toDate.Value);
            }

            // Pull data into memory to allow casting
            var payments = paymentsQuery.ToList();

            var model = new EarningsViewModel();

            if (groupBy == "Daily")
            {
                model.DailyEarnings = payments
                    .GroupBy(p => p.Paid_at.Date)
                    .Select(g => new DailyEarning
                    {
                        Date = g.Key,
                        TotalAmount = g.Sum(p => (decimal)p.Order.Total_ammount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(e => e.Date)
                    .Take(30)
                    .ToList();
            }
            else if (groupBy == "Monthly")
            {
                model.MonthlyEarnings = payments
                    .GroupBy(p => new { p.Paid_at.Year, p.Paid_at.Month })
                    .Select(g => new MonthlyEarning
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalAmount = g.Sum(p => (decimal)p.Order.Total_ammount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(e => e.Year)
                    .ThenBy(e => e.Month)
                    .Take(12)
                    .ToList();
            }
            else
            {
                // Show both views if no group is selected
                model.DailyEarnings = payments
                    .GroupBy(p => p.Paid_at.Date)
                    .Select(g => new DailyEarning
                    {
                        Date = g.Key,
                        TotalAmount = g.Sum(p => (decimal)p.Order.Total_ammount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(e => e.Date)
                    .Take(30)
                    .ToList();

                model.MonthlyEarnings = payments
                    .GroupBy(p => new { p.Paid_at.Year, p.Paid_at.Month })
                    .Select(g => new MonthlyEarning
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalAmount = g.Sum(p => (decimal)p.Order.Total_ammount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(e => e.Year)
                    .ThenBy(e => e.Month)
                    .Take(12)
                    .ToList();
            }

            model.TotalEarnings = payments.Sum(p => (decimal)p.Order.Total_ammount);
            model.TotalOrders = payments.Count;

            return View(model);
        }



        // GET: Admin/PaymentDetails/5
        public ActionResult PaymentDetails(int id)
        {
            var payment = db.Payments
                .Include(p => p.Order)
                .Include(p => p.Order.User)
                .Include(p => p.Order.OrderItems)
                .Include(p => p.Order.OrderItems.Select(oi => oi.Product))
                .FirstOrDefault(p => p.OrderId == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}