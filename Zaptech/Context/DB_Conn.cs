namespace Zaptech.Context
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Zaptech.Models;

    public class DB_Conn : DbContext
    {
        // Your context has been configured to use a 'DB_Conn' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Zaptech.Context.DB_Conn' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'DB_Conn' 
        // connection string in the application configuration file.
        public DB_Conn()
            : base("name=DB_Conn")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        //public virtual DbSet<MyEntity> MyEntities { get; set; }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Category> Categorys { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}