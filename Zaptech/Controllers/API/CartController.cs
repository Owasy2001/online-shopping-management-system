using Org.BouncyCastle.Asn1.Cmp;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Zaptech.Context;
using Zaptech.Models;

namespace Zaptech.Controllers.API
{
    [RoutePrefix("api/cart")]
    public class CartController : ApiController
    {
        private DB_Conn db;

        public CartController()
        {
            this.db = new DB_Conn();
        }

        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                // Authentication check
                var user = db.Users.Find(request.UserId);
                if (user == null || !user.IsActive)
                {
                    return Content(HttpStatusCode.Unauthorized, new
                    {
                        redirectUrl = "/Account/Login",
                        message = "Please login to continue"
                    });
                }

                // Product validation
                var product = db.Products
                    .FirstOrDefault(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                {
                    return BadRequest("Product not found");
                }

                // Stock validation
                if (product.Stock < request.Quantity)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        success = false,
                        redirectUrl = "/CustomerProducts/Details/" + request.ProductId,
                        message = $"Only {product.Stock} items available in stock"
                    });
                }

                // Get or create cart
                var cart = db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.UserId == request.UserId && o.Status == "Pending");

                if (cart == null)
                {
                    cart = new Order
                    {
                        UserId = request.UserId,
                        Status = "Pending",
                        Order_date = DateTime.Now,
                        Total_ammount = 0
                    };
                    db.Orders.Add(cart);
                    db.SaveChanges();
                }

                // Update or add item
                var existingItem = cart.OrderItems
                    .FirstOrDefault(oi => oi.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    cart.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = request.Quantity,
                        Price = product.Price
                    });
                }

                // Update stock
                product.Stock -= request.Quantity;

                // Recalculate total
                cart.Total_ammount = cart.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    redirectUrl = "/Customer/Dashboard",
                    cartItemCount = cart.OrderItems.Sum(oi => oi.Quantity),
                    message = $"{product.Name} added to cart successfully"
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    redirectUrl = "/CustomerProducts/Details/" + request.ProductId,
                    message = ex.Message
                });
            }
        }


        // GET: api/cart/{userId}
        [HttpGet]
        [Route("{userId}")]
        public IHttpActionResult GetCart(int userId)
        {
            var cart = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

            if (cart == null)
            {
                return NotFound();
            }

            return Ok(cart);
        }

        // POST: api/cart/apply-coupon
        [HttpPost]
        [Route("apply-coupon")]
        public IHttpActionResult ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var coupon = db.Coupons.FirstOrDefault(c => c.Code == request.CouponCode.ToUpper());
            if (coupon == null)
            {
                return BadRequest("Invalid coupon code");
            }

            if (!coupon.IsActive)
            {
                return BadRequest("This coupon is no longer active");
            }

            if (coupon.StartDate > DateTime.Now || coupon.EndDate < DateTime.Now)
            {
                return BadRequest("This coupon is expired");
            }

            if (coupon.MaxUses.HasValue && coupon.CurrentUses >= coupon.MaxUses.Value)
            {
                return BadRequest("This coupon has reached its maximum usage limit");
            }

            var cart = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.UserId == request.UserId && o.Status == "Pending");

            if (cart == null)
            {
                return BadRequest("No active cart found");
            }

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

            try
            {
                db.SaveChanges();
                return Ok(new
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
                return InternalServerError(ex);
            }
        }

        // POST: api/cart/remove-coupon
        [HttpPost]
        [Route("remove-coupon")]
        public IHttpActionResult RemoveCoupon([FromBody] RemoveCouponRequest request)
        {
            var cart = db.Orders
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.UserId == request.UserId && o.Status == "Pending");

            if (cart == null || cart.CouponId == null)
            {
                return BadRequest("No coupon applied to cart");
            }

            var coupon = db.Coupons.Find(cart.CouponId);
            if (coupon != null)
            {
                coupon.CurrentUses -= 1;
            }

            // Recalculate total without coupon
            decimal originalTotal = (decimal)cart.Total_ammount;
            if (coupon != null)
            {
                if (coupon.DiscountType == "Percentage")
                {
                    originalTotal = originalTotal / (1 - (coupon.DiscountValue / 100m));
                }
                else
                {
                    originalTotal = originalTotal + (decimal)coupon.DiscountValue;
                }
            }

            cart.CouponId = null;
            cart.Total_ammount = (float)originalTotal;

            try
            {
                db.SaveChanges();
                return Ok(new { success = true, newTotal = cart.Total_ammount });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/cart/update-item
        [HttpPost]
        [Route("update-item")]
        public IHttpActionResult UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            var item = db.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .FirstOrDefault(oi => oi.Id == request.ItemId && oi.Order.UserId == request.UserId);

            if (item == null)
            {
                return NotFound();
            }

            if (request.NewQuantity <= 0)
            {
                return BadRequest("Quantity must be at least 1");
            }

            if (item.Product.Stock < request.NewQuantity)
            {
                return BadRequest($"Only {item.Product.Stock} items available in stock");
            }

            try
            {
                // Update stock difference
                int quantityDifference = request.NewQuantity - item.Quantity;
                item.Product.Stock -= quantityDifference;

                item.Quantity = request.NewQuantity;
                item.Order.Total_ammount = item.Order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    itemTotal = item.Price * request.NewQuantity,
                    newTotal = item.Order.Total_ammount,
                    cartCount = item.Order.OrderItems.Sum(oi => oi.Quantity)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // In CartController.cs
        [HttpPost]
        [Route("remove-item")]
        public IHttpActionResult RemoveCartItem([FromBody] RemoveCartItemRequest request)
        {
            try
            {
                var item = db.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.Order)
                    .FirstOrDefault(oi => oi.Id == request.ItemId && oi.Order.UserId == request.UserId);

                if (item == null)
                {
                    return NotFound();
                }

                // Return the item quantity to stock
                item.Product.Stock += item.Quantity;

                // Remove the item
                db.OrderItems.Remove(item);

                // Recalculate order total
                item.Order.Total_ammount = item.Order.OrderItems
                    .Where(oi => oi.Id != request.ItemId)
                    .Sum(oi => oi.Quantity * oi.Price);

                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    newTotal = item.Order.Total_ammount,
                    cartCount = item.Order.OrderItems.Count - 1
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/cart/checkout
        [HttpPost]
        [Route("checkout")]
        public IHttpActionResult Checkout([FromBody] CheckoutRequest request)
        {
            var order = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == request.OrderId && o.UserId == request.UserId && o.Status == "Pending");

            if (order == null)
            {
                return BadRequest("No pending order found");
            }

            try
            {
                StripeConfiguration.ApiKey = "My Secreate !!!";

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(order.Total_ammount * 100),
                    Currency = "usd",
                    Description = "Order #" + order.Id,
                    PaymentMethod = request.StripeToken,
                    Confirm = true,
                    ReceiptEmail = request.UserEmail
                };

                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);

                order.Status = "Completed";

                var payment = new Payment
                {
                    OrderId = order.Id,
                    Status = "Paid",
                    Transiction_Id = paymentIntent.Id,
                    Paid_at = DateTime.Now
                };

                db.Payments.Add(payment);
                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    orderId = order.Id
                });
            }
            catch (StripeException e)
            {
                return BadRequest(e.StripeError.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    // Request models
    public class ApplyCouponRequest
    {
        public int UserId { get; set; }
        public string CouponCode { get; set; }
    }

    public class RemoveCouponRequest
    {
        public int UserId { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int NewQuantity { get; set; }
    }

  

    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string StripeToken { get; set; }
        public string UserEmail { get; set; }
    }

    // Add these classes to your CartController.cs
    public class AddToCartRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }

    // Add this class to your request models in CartController.cs
    public class RemoveCartItemRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
    }


}
