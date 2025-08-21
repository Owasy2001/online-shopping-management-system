using System;
using System.Data.Entity.Migrations;

namespace Zaptech.Migrations
{
    public partial class AddNafisaSalesData : DbMigration
    {
        public override void Up()
        {
            // Ensure Nafisa seller exists
            Sql(@"
                IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'nafisa.tn@gmail.com')
                BEGIN
                    INSERT INTO dbo.Users (Name, Email, Password, Role, Address, Phone)
                    VALUES 
                    ('Nafisa', 'nafisa.tn@gmail.com', 'nafisa123', '2', 'Dhaka', '01712345678');
                END
            ");

            // Get Nafisa's seller ID
            Sql(@"
                DECLARE @nafisaSellerId INT = (SELECT Id FROM Users WHERE Email = 'nafisa.tn@gmail.com');
                DECLARE @phoneCat INT = (SELECT Id FROM Categories WHERE Name = 'Phone');
                DECLARE @laptopCat INT = (SELECT Id FROM Categories WHERE Name = 'Laptop');
                DECLARE @headphonesCat INT = (SELECT Id FROM Categories WHERE Name = 'Headphones');
            ");

            // Add products for Nafisa
            Sql(@"
                DECLARE @nafisaSellerId INT = (SELECT Id FROM Users WHERE Email = 'nafisa.tn@gmail.com');
                DECLARE @phoneCat INT = (SELECT Id FROM Categories WHERE Name = 'Phone');
                DECLARE @laptopCat INT = (SELECT Id FROM Categories WHERE Name = 'Laptop');
                DECLARE @headphonesCat INT = (SELECT Id FROM Categories WHERE Name = 'Headphones');

                INSERT INTO dbo.Products (Name, Description, Price, Stock, Image, Brand_Name, Seller_ID, CategoryId) 
                VALUES
                ('iPhone 15 Pro', 'Latest Apple flagship with A17 chip', 1299.99, 15, 'https://i.ibb.co/iphone15.jpg', 'Apple', @nafisaSellerId, @phoneCat),
                ('Samsung Galaxy S24', 'Premium Android phone with Snapdragon 8 Gen 3', 1199.99, 12, 'https://i.ibb.co/s24.jpg', 'Samsung', @nafisaSellerId, @phoneCat),
                ('MacBook Pro M3', 'Professional laptop for creatives', 1999.99, 8, 'https://i.ibb.co/macpro.jpg', 'Apple', @nafisaSellerId, @laptopCat),
                ('Sony WH-1000XM6', 'Premium noise cancelling headphones', 399.99, 25, 'https://i.ibb.co/sonyxm6.jpg', 'Sony', @nafisaSellerId, @headphonesCat),
                ('Dell XPS 16', 'Ultra-slim laptop with OLED display', 1899.99, 6, 'https://i.ibb.co/dellxps.jpg', 'Dell', @nafisaSellerId, @laptopCat),
                ('Google Pixel 8 Pro', 'AI-powered phone with advanced camera', 999.99, 20, 'https://i.ibb.co/pixel8.jpg', 'Google', @nafisaSellerId, @phoneCat);
            ");

            // Add completed orders for Nafisa's products
            Sql(@"
                DECLARE @customer1 INT = (SELECT Id FROM Users WHERE Email = 'alice@email.com');
                DECLARE @customer2 INT = (SELECT Id FROM Users WHERE Email = 'bob@email.com');
                DECLARE @customer3 INT = (SELECT Id FROM Users WHERE Email = 'charlie@email.com');
                DECLARE @customer4 INT = (SELECT Id FROM Users WHERE Email = 'david@email.com');
                
                DECLARE @iphone INT = (SELECT Id FROM Products WHERE Name = 'iPhone 15 Pro');
                DECLARE @samsung INT = (SELECT Id FROM Products WHERE Name = 'Samsung Galaxy S24');
                DECLARE @macbook INT = (SELECT Id FROM Products WHERE Name = 'MacBook Pro M3');
                DECLARE @sony INT = (SELECT Id FROM Products WHERE Name = 'Sony WH-1000XM6');
                DECLARE @dell INT = (SELECT Id FROM Products WHERE Name = 'Dell XPS 16');
                DECLARE @pixel INT = (SELECT Id FROM Products WHERE Name = 'Google Pixel 8 Pro');
                
                INSERT INTO dbo.Orders (Order_date, Status, Total_ammount, UserId)
                VALUES
                (DATEADD(day, -30, GETDATE()), 'Completed', 3899.97, @customer1),
                (DATEADD(day, -25, GETDATE()), 'Completed', 2599.98, @customer2),
                (DATEADD(day, -20, GETDATE()), 'Completed', 1999.99, @customer3),
                (DATEADD(day, -15, GETDATE()), 'Completed', 399.99, @customer4),
                (DATEADD(day, -10, GETDATE()), 'Completed', 3799.98, @customer1),
                (DATEADD(day, -5, GETDATE()), 'Completed', 999.99, @customer2);
                
                DECLARE @order1 INT = SCOPE_IDENTITY() - 5;
                DECLARE @order2 INT = SCOPE_IDENTITY() - 4;
                DECLARE @order3 INT = SCOPE_IDENTITY() - 3;
                DECLARE @order4 INT = SCOPE_IDENTITY() - 2;
                DECLARE @order5 INT = SCOPE_IDENTITY() - 1;
                DECLARE @order6 INT = SCOPE_IDENTITY();
                
                INSERT INTO dbo.OrderItems (Quantity, Price, OrderId, ProductId)
                VALUES
                (1, 1299.99, @order1, @iphone),   -- iPhone 15 Pro
                (2, 1299.99, @order1, @macbook),  -- 2 MacBooks
                
                (1, 1199.99, @order2, @samsung),  -- Samsung Galaxy
                (1, 1399.99, @order2, @macbook),  -- MacBook
                
                (1, 1899.99, @order3, @dell),     -- Dell XPS
                (1, 100.00, @order3, @sony),      -- Sony Headphones
                
                (1, 399.99, @order4, @sony),      -- Sony Headphones
                
                (1, 1299.99, @order5, @iphone),   -- iPhone
                (1, 1299.99, @order5, @macbook),  -- MacBook
                (1, 1199.99, @order5, @dell),     -- Dell
                
                (1, 999.99, @order6, @pixel);     -- Google Pixel
            ");
        }

        public override void Down()
        {
            // Remove the added data
            Sql(@"
                -- Delete order items for Nafisa's products
                DELETE FROM OrderItems WHERE OrderId IN (
                    SELECT Id FROM Orders 
                    WHERE Status = 'Completed' 
                    AND Order_date > DATEADD(day, -35, GETDATE())
                );
                
                -- Delete orders for Nafisa's products
                DELETE FROM Orders 
                WHERE Status = 'Completed' 
                AND Order_date > DATEADD(day, -35, GETDATE());
                
                -- Delete Nafisa's products
                DELETE FROM Products 
                WHERE Seller_ID = (SELECT Id FROM Users WHERE Email = 'nafisa.tn@gmail.com')
                AND Name IN (
                    'iPhone 15 Pro', 
                    'Samsung Galaxy S24',
                    'MacBook Pro M3',
                    'Sony WH-1000XM6',
                    'Dell XPS 16',
                    'Google Pixel 8 Pro'
                );
                
                -- Delete Nafisa's account (only if we created it in this migration)
                IF NOT EXISTS (
                    SELECT * FROM Products WHERE Seller_ID = (SELECT Id FROM Users WHERE Email = 'nafisa.tn@gmail.com')
                )
                BEGIN
                    DELETE FROM Users WHERE Email = 'nafisa.tn@gmail.com';
                END
            ");
        }
    }
}
