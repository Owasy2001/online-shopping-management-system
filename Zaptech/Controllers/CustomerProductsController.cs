using System;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Models;
using PagedList;
using System.Data.Entity;
using Zaptech.Context;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Zaptech.Controllers.API;

namespace Zaptech.Controllers
{
    public class CustomerProductsController : Controller
    {
        private DB_Conn db = new DB_Conn();
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:50271/";

        // GET: CustomerProducts
        public ActionResult Index(string searchString, string categoryFilter,
                                float? minPrice, float? maxPrice,
                                string sortOrder, int? page)
        {
            // Sorting parameters
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.CurrentFilter = searchString;
            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            IQueryable<Product> products = db.Products.Include(p => p.Category);

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) ||
                                      p.Category.Name.Contains(searchString));
            }

            // Category filter
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                products = products.Where(p => p.Category.Name == categoryFilter);
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Sorting
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "Price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Get distinct categories for dropdown
            ViewBag.Categories = db.Categorys
                                .Where(c => c.Status == "Active") // Only active categories
                                .Select(c => c.Name)
                                .Distinct()
                                .OrderBy(c => c)
                                .ToList();

            // Pagination - 6 items per page
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // Add this method to both controllers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new
                    {
                        redirectUrl = Url.Action("Login", "Account", new { returnUrl = Request.Url?.ToString() })
                    });
                }

                int userId = (int)Session["UserId"];

                // Get product
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Check stock
                if (product.Stock < quantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Only {product.Stock} items available in stock"
                    });
                }

                // Get or create cart
                var cart = db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

                if (cart == null)
                {
                    cart = new Order
                    {
                        UserId = userId,
                        Status = "Pending",
                        Order_date = DateTime.Now,
                        Total_ammount = 0
                    };
                    db.Orders.Add(cart);
                }

                // Check if item already in cart
                var existingItem = cart.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        Price = product.Price
                    });
                }

                // Update stock
                product.Stock -= quantity;

                // Update total
                cart.Total_ammount = (float)cart.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    cartItemCount = cart.OrderItems.Sum(oi => oi.Quantity),
                    message = $"{product.Name} added to cart successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error adding to cart: " + ex.Message
                });
            }
        }



        // GET: CustomerProducts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.Reviews.Select(r => r.User))
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Calculate average rating
            if (product.Reviews != null && product.Reviews.Any())
            {
                ViewBag.AverageRating = Math.Round(product.Reviews.Average(r => r.Rating), 1);
                ViewBag.ReviewCount = product.Reviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            return View(product);
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