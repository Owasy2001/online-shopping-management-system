using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;
using Review = Zaptech.Models.Review;

namespace Zaptech.Controllers
{
    [CustomerAuthorization]
    public class CustomerController : Controller
    {
        private DB_Conn db = new DB_Conn();
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:50271/";

        // GET: Customer/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get cart from API with fallback to DB
            Order cart = await GetCartFromApiWithFallback(userId);

            // Get user's reviews with product information
            var userReviews = db.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Created_at)
                .Take(5) // Show only 5 most recent reviews
                .ToList();

            var recentOrders = db.Orders
                .Where(o => o.UserId == userId && o.Status == "Completed")
                .OrderByDescending(o => o.Order_date)
                .Take(5)
                .ToList();

            var model = new CustomerDashboardViewModel
            {
                User = user,
                Orders = recentOrders,
                CartItemCount = cart?.OrderItems.Sum(oi => oi.Quantity) ?? 0,
                CartTotalAmount = (decimal)(cart?.Total_ammount ?? 0f),
                UserReviews = userReviews, // Add reviews to the view model
                CurrentCart = cart
            };

            return View(model);
        }


      

        [HttpGet]
        public ActionResult GetCart()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];

            // Get cart with all necessary includes
            var cart = db.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

            if (cart == null)
            {
                return PartialView("_EmptyCart");
            }

            return PartialView("_CartItems", cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyCoupon(string couponCode)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            int userId = (int)Session["UserId"];
            var cart = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

            if (cart == null || cart.OrderItems.Count == 0)
            {
                return Json(new { success = false, message = "Your cart is empty" });
            }

            var coupon = db.Coupons.FirstOrDefault(c => c.Code == couponCode.ToUpper());
            if (coupon == null)
            {
                return Json(new { success = false, message = "Invalid coupon code" });
            }

            // Validate coupon
            if (!coupon.IsActive)
            {
                return Json(new { success = false, message = "This coupon is no longer active" });
            }

            if (coupon.StartDate > DateTime.Now || coupon.EndDate < DateTime.Now)
            {
                return Json(new { success = false, message = "This coupon is expired" });
            }

            if (coupon.MaxUses.HasValue && coupon.CurrentUses >= coupon.MaxUses.Value)
            {
                return Json(new { success = false, message = "This coupon has reached its maximum usage limit" });
            }

            // Check minimum order amount
            if (coupon.MinimumOrderAmount.HasValue &&
                (decimal)cart.Total_ammount < coupon.MinimumOrderAmount.Value)
            {
                return Json(new
                {
                    success = false,
                    message = $"Minimum order amount of {coupon.MinimumOrderAmount.Value:C} required for this coupon"
                });
            }

            try
            {
                // Apply coupon
                cart.CouponId = coupon.Id;
                coupon.CurrentUses += 1;

                decimal discountAmount = 0;
                if (coupon.DiscountType == "Percentage")
                {
                    discountAmount = (decimal)(cart.Total_ammount) * (coupon.DiscountValue / 100m);
                }
                else
                {
                    discountAmount = (decimal)coupon.DiscountValue;
                }

                cart.Total_ammount = (float)((decimal)(cart.Total_ammount) - discountAmount);

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    newTotal = cart.Total_ammount,
                    couponCode = coupon.Code,
                    discountType = coupon.DiscountType,
                    discountValue = coupon.DiscountValue
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error applying coupon: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCartItem(int itemId, int newQuantity)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Session expired" });
            }

            var item = db.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .FirstOrDefault(oi => oi.Id == itemId && oi.Order.UserId == (int)Session["UserId"]);

            if (item == null)
            {
                return Json(new { success = false, message = "Item not found in cart" });
            }

            if (newQuantity <= 0)
            {
                return Json(new { success = false, message = "Quantity must be at least 1" });
            }

            if (newQuantity > item.Product.Stock + item.Quantity) // Check available stock
            {
                return Json(new
                {
                    success = false,
                    message = $"Only {item.Product.Stock} items available in stock"
                });
            }

            try
            {
                // Calculate stock difference
                int quantityDifference = newQuantity - item.Quantity;
                item.Product.Stock -= quantityDifference;

                item.Quantity = newQuantity;

                // Recalculate order total
                item.Order.Total_ammount = (float)item.Order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    itemTotal = item.Price * newQuantity,
                    newTotal = item.Order.Total_ammount,
                    cartCount = item.Order.OrderItems.Sum(oi => oi.Quantity)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // In CustomerController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveFromCart(int itemId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Session expired" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.PostAsJsonAsync("api/cart/remove-item", new
                    {
                        UserId = (int)Session["UserId"],
                        ItemId = itemId
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<dynamic>();
                        return Json(new
                        {
                            success = true,
                            newTotal = result.newTotal,
                            cartCount = result.cartCount
                        });
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout(string stripeToken)
        {
            if (Session["UserId"] == null || Session["UserEmail"] == null)
            {
                return Json(new { success = false, message = "Session expired" });
            }

            int userId = (int)Session["UserId"];

            // First get the order ID from the cart
            var cart = await GetCartFromApiWithFallback(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "No pending order found" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.PostAsJsonAsync("api/cart/checkout", new
                    {
                        UserId = userId,
                        OrderId = cart.Id,
                        StripeToken = stripeToken,
                        UserEmail = Session["UserEmail"].ToString()
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<dynamic>();
                        return Json(new
                        {
                            success = true,
                            redirectUrl = Url.Action("OrderDetails", new { id = result.orderId })
                        });
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Customer/Checkout
        public async Task<ActionResult> Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var cart = await GetCartFromApiWithFallback(userId);

            if (cart == null || cart.OrderItems.Count == 0)
            {
                return RedirectToAction("Index", "CustomerHome");
            }

            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["StripePublishableKey"];
            return View(cart);
        }

        // GET: Customer/OrderHistory
        public ActionResult OrderHistory()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var orders = db.Orders
                .Where(o => o.UserId == userId && (o.Status == "Completed" || o.Status == "Pending"))
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.Payment)
                .ToList();

            return View(orders);
        }

        // GET: Customer/AddReview/5
        public ActionResult AddReview(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var orderItem = db.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefault(oi => oi.Id == id && oi.Order.UserId == userId);

            if (orderItem == null)
            {
                return HttpNotFound();
            }

            // Check if user has already reviewed this product
            bool hasReviewed = db.Reviews.Any(r => r.UserId == userId && r.ProductId == orderItem.ProductId);
            if (hasReviewed)
            {
                TempData["Message"] = "You have already reviewed this product.";
                return RedirectToAction("OrderDetails", new { id = orderItem.OrderId });
            }

            var model = new AddReviewViewModel
            {
                OrderItemId = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.Name,
                ProductImage = orderItem.Product.Image
            };

            return View(model);
        }

        // POST: Customer/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddReview(AddReviewViewModel model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = (int)Session["UserId"];

            // Verify the user actually ordered this product
            var orderItem = db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefault(oi => oi.Id == model.OrderItemId && oi.Order.UserId == userId);

            if (orderItem == null)
            {
                return HttpNotFound();
            }

            // Check if review already exists
            if (db.Reviews.Any(r => r.UserId == userId && r.ProductId == model.ProductId))
            {
                TempData["Message"] = "You have already reviewed this product.";
                return RedirectToAction("OrderDetails", new { id = orderItem.OrderId });
            }

            var review = new Review
            {
                ProductId = model.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                Created_at = DateTime.Now
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["Success"] = "Thank you for your review!";
            return RedirectToAction("OrderDetails", new { id = orderItem.OrderId });
        }

        // GET: Customer/MyReviews
        public ActionResult MyReviews()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var reviews = db.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Created_at)
                .ToList();

            return View(reviews);
        }

        // GET: Customer/EditProfile
        public ActionResult EditProfile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            return View(user);
        }

        // POST: Customer/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                // Update session
                Session["UserName"] = user.Name;

                return RedirectToAction("Dashboard");
            }
            return View(user);
        }

        public ActionResult OrderDetails(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];

            // Disable proxy creation for this query
            db.Configuration.ProxyCreationEnabled = false;

            var order = db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.OrderItems.Select(oi => oi.Product.Category))
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Create a dictionary to track which products have been reviewed
            var hasReviews = new Dictionary<int, bool>();
            foreach (var item in order.OrderItems)
            {
                hasReviews[item.ProductId] = db.Reviews.Any(r => r.UserId == userId && r.ProductId == item.ProductId);
            }

            ViewBag.HasReviews = hasReviews;

            // Re-enable proxy creation for other operations
            db.Configuration.ProxyCreationEnabled = true;

            return View(order);
        }

        // GET: Customer/DownloadReceipt/5
        public ActionResult DownloadReceipt(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            var order = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return new Rotativa.ViewAsPdf("Receipt", order)
            {
                FileName = $"Receipt_{order.Id}_{DateTime.Now:yyyyMMdd}.pdf"
            };
        }

        private async Task<Order> GetCartFromApiWithFallback(int userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.GetAsync($"api/cart/{userId}");

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<Order>();
                    }
                }
            }
            catch
            {
                // Log the error if needed
            }

            // Fallback to direct DB access
            return db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
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