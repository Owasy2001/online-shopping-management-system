using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Models;
using Zaptech.Models.ViewModels;

namespace Zaptech.Controllers
{
    public class CustomerHomeController : Controller
    {
        private DB_Conn db = new DB_Conn();
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:50271/";

        // GET: CustomerHome
        public ActionResult Index()
        {
            try
            {
                // Get phone category ID (assuming it exists)
                var phoneCategoryId = db.Categorys
                    .Where(c => c.Name == "Phone")
                    .Select(c => c.Id)
                    .FirstOrDefault();

                var model = new LandingPageViewModel
                {
                    // Banner data
                    Banners = new[]
                    {
                        new Banner
                        {
                            Title = "Latest Smartphones",
                            Description = "Discover our newest collection of cutting-edge smartphones",
                            ImageUrl = "/Images/ban1.png",
                            CtaText = "Shop Now"
                        },
                        new Banner
                        {
                            Title = "Premium Laptops",
                            Description = "Powerful laptops for work and play",
                            ImageUrl = "/Images/ban2.png",
                            CtaText = "Explore"
                        },
                        //new Banner
                        //{
                        //    Title = "Smart Accessories",
                        //    Description = "Enhance your tech experience with our accessories",
                        //    ImageUrl = "/Content/Images/banner3.jpg",
                        //    CtaText = "Discover"
                        //}
                    },

                    // Active categories
                    Categories = db.Categorys
                        .Where(c => c.Status == "Active")
                        .OrderBy(c => c.Name)
                        .ToList(),

                    // Recently added products (with 5% discount)
                    RecentProducts = db.Products
                        .OrderByDescending(p => p.Id)
                        .Take(4)
                        .ToList(),

                    // Top 3 most expensive products
                    TopExpensiveProducts = db.Products
                        .OrderByDescending(p => p.Price)
                        .Take(3)
                        .ToList(),

                    // Top 3 reviews
                    TopReviews = db.Reviews
                        .Include(r => r.Product)
                        .Include(r => r.User)
                        .OrderByDescending(r => r.Rating)
                        .Take(3)
                        .ToList(),

                    // FAQ data
                    Faqs = new[]
                    {
                        new Faq { Question = "How can I track my order?", Answer = "Use the tracking number in your confirmation email." },
                        new Faq { Question = "What is your return policy?", Answer = "30-day return policy with original receipt." },
                        new Faq { Question = "Do you offer international shipping?", Answer = "Yes, we ship worldwide with additional fees." }
                    },

                    // Default category products (Phone)
                    DefaultCategoryProducts = db.Products
                        .Where(p => p.CategoryId == phoneCategoryId)
                        .Take(3)
                        .ToList(),

                    DefaultCategoryId = phoneCategoryId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log error (implement proper logging)
                // Return error view or redirect
                return View("Error");
            }
        }



      

        // GET: Category products (for AJAX)
        public ActionResult CategoryProducts(int id)
        {
            try
            {
                var products = db.Products
                    .Where(p => p.CategoryId == id)
                    .Take(3)
                    .ToList();

                return PartialView("_CategoryProducts", products);
            }
            catch
            {
                return Content("<div class='alert alert-danger'>Error loading products</div>");
            }
        }

        // GET: Product modal content
        public ActionResult ProductModal(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                    return Content("<div class='alert alert-warning'>Product not found</div>");

                return PartialView("_ProductModal", product);
            }
            catch
            {
                return Content("<div class='alert alert-danger'>Error loading product details</div>");
            }
        }

        // GET: Product details page
        public ActionResult ProductDetails(int id)
        {
            try
            {
                var product = db.Products
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                    return HttpNotFound();

                // Check if product is recently added (for discount)
                var isRecent = db.Products
                    .OrderByDescending(p => p.Id)
                    .Take(10)
                    .Any(p => p.Id == id);

                if (isRecent)
                {
                    ViewBag.Discount = 0.05;
                    ViewBag.DiscountedPrice = product.Price * 0.95;
                }

                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", new { error = "Error loading product" });
            }
        }

        // GET: Expensive product details
        public ActionResult ExpensiveProductDetails(int id)
        {
            try
            {
                var product = db.Products
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                    return HttpNotFound();

                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", new { error = "Error loading product details" });
            }
        }

        // GET: Coupon redirect
        public ActionResult GetCoupon()
        {
            return RedirectToAction("Login", "Account");
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
