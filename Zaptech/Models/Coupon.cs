using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
	public class Coupon
	{
        public int Id { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "Discount Type")]
        public string DiscountType { get; set; } 

        [Required]
        [Display(Name = "Discount Value")]
        [Range(0.01, 10000)]
        public decimal DiscountValue { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(5);


        [Display(Name = "Minimum Order Amount")]
        [Range(0, 100000)]
        public decimal? MinimumOrderAmount { get; set; }

        [Display(Name = "Max Uses")]
        public int? MaxUses { get; set; }

        [Display(Name = "Current Uses")]
        public int CurrentUses { get; set; } = 0;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

       
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}