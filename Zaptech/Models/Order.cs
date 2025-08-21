using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
	public class Order
	{
        public int Id { get; set; }

        public DateTime Order_date { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; }

        [Range(0.01, 100000)]
        public float Total_ammount { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }


        public int? CouponId { get; set; } 
        public Coupon Coupon { get; set; } 

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment Payment { get; set; }
    }
}