using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
	public class OrderItem
	{
        public int Id { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Range(0.01, 100000)]
        public float Price { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}