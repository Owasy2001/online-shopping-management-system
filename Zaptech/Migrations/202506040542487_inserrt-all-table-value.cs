namespace Zaptech.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inserrtalltablevalue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Status = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        Description = c.String(maxLength: 1000),
                        Price = c.Single(nullable: false),
                        Stock = c.Int(nullable: false),
                        Image = c.String(maxLength: 300),
                        Brand_Name = c.String(maxLength: 100),
                        Seller_ID = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        Price = c.Single(nullable: false),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Order_date = c.DateTime(nullable: false),
                        Status = c.String(maxLength: 50),
                        Total_ammount = c.Single(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        OrderId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 50),
                        Transiction_Id = c.String(nullable: false, maxLength: 100),
                        Paid_at = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        Password = c.String(nullable: false, maxLength: 100),
                        Role = c.String(nullable: false),
                        Address = c.String(maxLength: 200),
                        Phone = c.String(maxLength: 15),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(maxLength: 1000),
                        Created_at = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ProductId);


            Sql(@"
INSERT INTO dbo.Users (Name, Email, Password, Role, Address, Phone)
VALUES 
('Alice', 'alice@email.com', 'password1', 'Customer', 'Dhaka', '01700000001'),
('Bob', 'bob@email.com', 'password2', 'Customer', 'Chattogram', '01700000002'),
('Charlie', 'charlie@email.com', 'password3', 'Customer', 'Sylhet', '01700000003'),
('David', 'david@email.com', 'password4', 'Customer', 'Barishal', '01700000004'),
('Emma', 'emma@email.com', 'password5', 'Customer', 'Rajshahi', '01700000005'),
('Fahim', 'fahim@email.com', 'password6', 'Customer', 'Rangpur', '01700000006'),
('Gita', 'gita@email.com', 'password7', 'Customer', 'Khulna', '01700000007'),
('Hasan', 'hasan@email.com', 'password8', 'Customer', 'Cumilla', '01700000008'),
('Ivy', 'ivy@email.com', 'password9', 'Customer', 'Gazipur', '01700000009'),
('samio', 'samiohasan6@gmail.com', 'samio123', 'Customer', 'Chuadanga', '01709801305');
");

            Sql(@"
INSERT INTO dbo.Categories (Name, Status)
VALUES 
('Phone', 'Active'),
('Laptop', 'Active'),
('Tablet', 'Active'),
('Headphones', 'Active'),
('Smartwatch', 'Active');
");

            Sql(@"
INSERT INTO dbo.Products (Name, Description, Price, Stock, Image, Brand_Name, Seller_ID, CategoryId) VALUES
-- Phones
('iPhone 14 Pro', 'Apple flagship phone with A16 chip', 1299.99, 10, 'https://i.ibb.co/BxyJ4PN/p1.jpg', 'Apple', 1, 1),
('Samsung Galaxy S23', 'Samsung phone with Snapdragon 8 Gen 2', 1199.99, 15, 'https://i.ibb.co/s9H3sh4h/p2.jpg', 'Samsung', 2, 1),
('Google Pixel 7', 'Android phone with Tensor chip', 899.99, 8, 'https://i.ibb.co/GfDN98zJ/p3.jpg', 'Google', 3, 1),
('OnePlus 11', 'Flagship killer Android phone', 799.99, 12, 'https://i.ibb.co/ynNL2nYq/p4.jpg', 'OnePlus', 4, 1),
('Xiaomi 13 Pro', 'Affordable high-end phone', 699.99, 20, 'https://i.ibb.co/QFHGZxgY/p5.jpg', 'Xiaomi', 5, 1),
('Motorola Edge 30', 'Mid-range Android phone', 599.99, 10, 'https://i.ibb.co/gMLHQLBY/p6.jpg', 'Motorola', 6, 1),

-- Laptops
('MacBook Air M2', 'Ultra-thin laptop with Apple Silicon', 1299.99, 5, 'https://i.ibb.co/BVjTZtt3/l1.jpg', 'Apple', 1, 2),
('Dell XPS 15', 'Powerful Windows laptop', 1499.99, 7, 'https://i.ibb.co/270t7W4W/l2.jpg', 'Dell', 2, 2),
('HP Spectre x360', 'Convertible touch laptop', 1199.99, 10, 'https://i.ibb.co/0pRCXS8w/l3.jpg', 'HP', 3, 2),
('Lenovo Yoga 9i', 'Touchscreen 2-in-1 laptop', 1099.99, 8, 'https://i.ibb.co/spkJ1mgV/l4.jpg', 'Lenovo', 4, 2),
('Asus ROG Zephyrus', 'Gaming laptop with RTX 4070', 1999.99, 6, 'https://i.ibb.co/7N2fZV6B/l5.jpg', 'Asus', 5, 2),
('MSI Creator Z16', 'Content creator laptop', 1899.99, 4, 'https://i.ibb.co/RptP5bY4/l6.jpg', 'MSI', 6, 2),

-- Tablets
('iPad Pro 11', 'Apple tablet with M2 chip', 999.99, 9, 'https://i.ibb.co/bM29tChw/i1.jpg', 'Apple', 1, 3),
('Samsung Galaxy Tab S8', 'Android tablet for productivity', 849.99, 11, 'https://i.ibb.co/jpQKBwV/i2.jpg', 'Samsung', 2, 3),
('Lenovo Tab P12 Pro', 'Tablet for entertainment', 699.99, 10, 'https://i.ibb.co/4gZTPwH4/i3.jpg', 'Lenovo', 3, 3),
('Microsoft Surface Pro 9', 'Hybrid laptop-tablet', 1399.99, 7, 'https://i.ibb.co/BVrYcLv1/i4.jpg', 'Microsoft', 4, 3),
('Xiaomi Pad 6', 'Budget tablet for media', 499.99, 12, 'https://i.ibb.co/XGdC9Mx/i5.jpg', 'Xiaomi', 5, 3),
('Huawei MatePad 11', 'Android tablet with pen support', 649.99, 8, 'https://i.ibb.co/Cp7FtDPN/i6.jpg', 'Huawei', 6, 3),

-- Headphones
('AirPods Pro 2', 'Noise-cancelling Apple earbuds', 249.99, 15, 'https://i.ibb.co/MTHdbpb/a1.jpg', 'Apple', 1, 4),
('Sony WH-1000XM5', 'Best noise cancelling headphones', 349.99, 10, 'https://i.ibb.co/SXfk2B0N/a2.jpg', 'Sony', 2, 4),
('Bose QuietComfort 45', 'Premium over-ear headphones', 329.99, 9, 'https://i.ibb.co/dwPhVNpH/a3.jpg', 'Bose', 3, 4),
('Samsung Galaxy Buds 2 Pro', 'Wireless earbuds for Android', 199.99, 14, 'https://i.ibb.co/s8RgYhc/a4.jpg', 'Samsung', 4, 4),
('Beats Studio Buds+', 'Stylish wireless earbuds', 179.99, 18, 'https://i.ibb.co/5W2LNzP1/a5.jpg', 'Beats', 5, 4),
('JBL Tune 760NC', 'Affordable noise-cancelling headphones', 129.99, 16, 'https://i.ibb.co/HLJcmn6G/a6.jpg', 'JBL', 6, 4),

-- Smartwatches
('Apple Watch Series 9', 'Health-focused smartwatch', 399.99, 13, 'https://i.ibb.co/0jtv5FqF/w1.jpg', 'Apple', 1, 5),
('Samsung Galaxy Watch 6', 'Advanced health tracking watch', 349.99, 11, 'https://i.ibb.co/xSnsLQv6/w2.jpg', 'Samsung', 2, 5),
('Fitbit Versa 4', 'Fitness tracking watch', 229.99, 10, 'https://i.ibb.co/cKJB6XQw/w3.jpg', 'Fitbit', 3, 5),
('Garmin Venu 2', 'GPS smartwatch for runners', 399.99, 9, 'https://i.ibb.co/4w9XMFb7/w4.jpg', 'Garmin', 4, 5),
('Amazfit GTR 4', 'Stylish budget smartwatch', 179.99, 20, 'https://i.ibb.co/wFf5rRP1/w5.jpg', 'Amazfit', 5, 5),
('Realme Watch 3 Pro', 'Affordable smartwatch for beginners', 129.99, 25, 'https://i.ibb.co/DDZYjxBk/w6.jpg', 'Realme', 6, 5);

");

            Sql(@"
INSERT INTO dbo.Orders (Order_date, Status, Total_ammount, UserId)
VALUES
(GETDATE(), 'Pending', 1099.99, 1),
(GETDATE(), 'Completed', 249.99, 2),
(GETDATE(), 'Shipped', 499.99, 3),
(GETDATE(), 'Pending', 199.99, 4),
(GETDATE(), 'Completed', 899.99, 5),
(GETDATE(), 'Pending', 149.99, 6),
(GETDATE(), 'Delivered', 349.99, 7),
(GETDATE(), 'Completed', 699.99, 8),
(GETDATE(), 'Processing', 999.99, 9),
(GETDATE(), 'Pending', 1299.99, 10);
");

            Sql(@"
INSERT INTO dbo.OrderItems (Quantity, Price, OrderId, ProductId)
VALUES
(1, 999.99, 1, 1),
(2, 249.99, 2, 10),
(1, 799.99, 3, 3),
(3, 199.99, 4, 5),
(1, 899.99, 5, 2),
(2, 149.99, 6, 6),
(1, 349.99, 7, 9),
(1, 699.99, 8, 4),
(2, 999.99, 9, 7),
(1, 1299.99, 10, 8);
");

            Sql(@"
INSERT INTO dbo.Payments (OrderId, Status, Transiction_Id, Paid_at)
VALUES
(1, 'Paid', 'TXN1001', GETDATE()),
(2, 'Paid', 'TXN1002', GETDATE()),
(3, 'Unpaid', 'TXN1003', GETDATE()),
(4, 'Paid', 'TXN1004', GETDATE()),
(5, 'Paid', 'TXN1005', GETDATE()),
(6, 'Pending', 'TXN1006', GETDATE()),
(7, 'Paid', 'TXN1007', GETDATE()),
(8, 'Paid', 'TXN1008', GETDATE()),
(9, 'Unpaid', 'TXN1009', GETDATE()),
(10, 'Paid', 'TXN1010', GETDATE());
");
            Sql(@"
INSERT INTO dbo.Reviews (Rating, Comment, Created_at, UserId, ProductId)
VALUES
(5, 'Excellent product!', GETDATE(), 1, 1),
(4, 'Very good', GETDATE(), 2, 2),
(3, 'Average', GETDATE(), 3, 3),
(5, 'Loved it!', GETDATE(), 4, 4),
(2, 'Not what I expected', GETDATE(), 5, 5),
(4, 'Nice quality', GETDATE(), 6, 6),
(5, 'Highly recommended', GETDATE(), 7, 7),
(3, 'It’s okay', GETDATE(), 8, 8),
(4, 'Good product', GETDATE(), 9, 9),
(5, 'Best purchase ever!', GETDATE(), 10, 10);
");




        }

        public override void Down()
        {
            DropForeignKey("dbo.OrderItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Reviews", "UserId", "dbo.Users");
            DropForeignKey("dbo.Reviews", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Payments", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Reviews", new[] { "ProductId" });
            DropIndex("dbo.Reviews", new[] { "UserId" });
            DropIndex("dbo.Payments", new[] { "OrderId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.OrderItems", new[] { "ProductId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropTable("dbo.Reviews");
            DropTable("dbo.Users");
            DropTable("dbo.Payments");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
