using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Zaptech.Models
{
	public class Category
	{

        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}