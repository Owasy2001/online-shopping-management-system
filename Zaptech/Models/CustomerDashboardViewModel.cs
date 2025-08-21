
using System.Collections.Generic;
namespace Zaptech.Models 
{
    public class CustomerDashboardViewModel
    {
        public User User { get; set; }
        public ICollection<Order> Orders { get; set; }
        public int CartItemCount { get; set; }
        public decimal CartTotalAmount { get; set; }
        public ICollection<Coupon> ActiveCoupons { get; set; }

        public List<Review> UserReviews { get; set; }
        public Order CurrentCart { get; set; }
    }
}