using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;

namespace Zaptech.Controllers
{
    [SellerAuthorization]
    public class SellerProductsController : Controller
    {
        private DB_Conn db = new DB_Conn();

        // GET: SellerStats/TotalProducts
        public ActionResult TotalProducts(int? page)
        {
            int sellerId = (int)Session["UserId"];
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId)
                .OrderBy(p => p.Name)
                .ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        // GET: SellerStats/ActiveProducts
        public ActionResult ActiveProducts(int? page)
        {
            int sellerId = (int)Session["UserId"];
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        // GET: SellerStats/OutOfStockProducts
        public ActionResult OutOfStockProducts(int? page)
        {
            int sellerId = (int)Session["UserId"];
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId && p.Stock <= 0)
                .OrderBy(p => p.Name)
                .ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        // GET: SellerStats/PendingOrders
        public ActionResult PendingOrders(int? page)
        {
            int sellerId = (int)Session["UserId"];
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Where(o => o.Status == "Pending" &&
                            o.OrderItems.Any(oi => oi.Product.Seller_ID == sellerId))
                .OrderByDescending(o => o.Order_date)
                .ToPagedList(pageNumber, pageSize);

            return View(orders);
        }

        // PDF Export Actions
        public ActionResult ExportTotalProductsPDF()
        {
            int sellerId = (int)Session["UserId"];
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId)
                .OrderBy(p => p.Name)
                .ToList();

            return GenerateProductsPDF(products, "Total Products Report");
        }

        public ActionResult ExportActiveProductsPDF()
        {
            int sellerId = (int)Session["UserId"];
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToList();

            return GenerateProductsPDF(products, "Active Products Report");
        }

        public ActionResult ExportOutOfStockProductsPDF()
        {
            int sellerId = (int)Session["UserId"];
            var products = db.Products
                .Include(p => p.Category)
                .Where(p => p.Seller_ID == sellerId && p.Stock <= 0)
                .OrderBy(p => p.Name)
                .ToList();

            return GenerateProductsPDF(products, "Out-of-Stock Products Report");
        }

        public ActionResult ExportPendingOrdersPDF()
        {
            int sellerId = (int)Session["UserId"];
            var orders = db.Orders
                .Include(o => o.User)
                .Where(o => o.Status == "Pending" &&
                            o.OrderItems.Any(oi => oi.Product.Seller_ID == sellerId))
                .OrderByDescending(o => o.Order_date)
                .ToList();

            return GenerateOrdersPDF(orders, "Pending Orders Report");
        }

        private FileResult GenerateProductsPDF(List<Product> products, string title)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title and Seller Info
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Report title
                Paragraph docTitle = new Paragraph(title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(docTitle);

                // Seller info
                var seller = db.Users.Find((int)Session["UserId"]);
                Paragraph sellerInfo = new Paragraph($"Seller: {seller.Name}\nGenerated on: {DateTime.Now:dd MMM yyyy}", cellFont)
                {
                    SpacingAfter = 15f
                };
                document.Add(sellerInfo);

                // Create table
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3, 2, 1, 1, 2 }); // Column widths

                // Table headers
                string[] headers = { "Product", "Category", "Price", "Stock", "Status" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230), // Light gray
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Table data
                foreach (var product in products)
                {
                    table.AddCell(new Phrase(product.Name, cellFont));
                    table.AddCell(new Phrase(product.Category?.Name ?? "N/A", cellFont));
                    table.AddCell(new Phrase($"${product.Price:0.00}", cellFont));
                    table.AddCell(new Phrase(product.Stock.ToString(), cellFont));

                    // Status with conditional coloring
                    PdfPCell statusCell = new PdfPCell(new Phrase(product.IsActive ? "Active" : "Inactive", cellFont))
                    {
                        BackgroundColor = product.IsActive
                            ? new BaseColor(144, 238, 144) // Light green
                            : new BaseColor(255, 182, 193) // Light pink
                    };
                    table.AddCell(statusCell);
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"{title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        private FileResult GenerateOrdersPDF(List<Order> orders, string title)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title and Seller Info
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Report title
                Paragraph docTitle = new Paragraph(title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(docTitle);

                // Seller info
                var seller = db.Users.Find((int)Session["UserId"]);
                Paragraph sellerInfo = new Paragraph($"Seller: {seller.Name}\nGenerated on: {DateTime.Now:dd MMM yyyy}", cellFont)
                {
                    SpacingAfter = 15f
                };
                document.Add(sellerInfo);

                // Create table
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 2, 2, 2, 2 }); // Column widths

                // Table headers
                string[] headers = { "Order #", "Customer", "Date", "Total", "Status" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230), // Light gray
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Table data
                foreach (var order in orders)
                {
                    table.AddCell(new Phrase($"ZT-{order.Id}", cellFont));
                    table.AddCell(new Phrase(order.User?.Name ?? "N/A", cellFont));
                    table.AddCell(new Phrase(order.Order_date.ToString("dd MMM yyyy"), cellFont));
                    table.AddCell(new Phrase($"${order.Total_ammount:0.00}", cellFont));

                    // Status with conditional coloring
                    PdfPCell statusCell = new PdfPCell(new Phrase(order.Status, cellFont))
                    {
                        BackgroundColor = order.Status == "Pending"
                            ? new BaseColor(255, 255, 0) // Yellow
                            : new BaseColor(144, 238, 144) // Light green
                    };
                    table.AddCell(statusCell);
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"{title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}