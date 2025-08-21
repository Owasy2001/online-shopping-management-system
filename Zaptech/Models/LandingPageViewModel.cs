using System.Collections.Generic;
using Zaptech.Models;

namespace Zaptech.Models.ViewModels
{
    public class LandingPageViewModel
    {
        public Banner[] Banners { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Product> RecentProducts { get; set; }
        public ICollection<Product> TopExpensiveProducts { get; set; }
        public List<Product> Products { get; set; } 
        public ICollection<Review> TopReviews { get; set; }
        public Faq[] Faqs { get; set; }
        public ICollection<Product> DefaultCategoryProducts { get; set; }
        public int DefaultCategoryId { get; set; }
    }

    public class Banner
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string CtaText { get; set; }
    }

    public class Faq
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}