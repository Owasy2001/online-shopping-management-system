using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Zaptech.Models
{
	public class Payment
	{
        [Key, ForeignKey("Order")] 
        public int OrderId { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; }

        [Required, StringLength(100)]
        public string Transiction_Id { get; set; }

        public DateTime Paid_at { get; set; } = DateTime.Now;

        public Order Order { get; set; }
    }
}