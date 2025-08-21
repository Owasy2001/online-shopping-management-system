using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zaptech.Models
{
   

    public enum UserRole
    {
        Customer = 1,
        Seller = 2,
        Admin = 99 
    }

   
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string StoreName { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }

       
        public bool IsActive { get; set; }
    }
}



