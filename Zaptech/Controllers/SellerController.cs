using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;
using PagedList;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
 

namespace Zaptech.Controllers
{
    [SellerAuthorization]
    public class SellerController : Controller
    {
        private DB_Conn db = new DB_Conn();

        // GET: Seller/Dashboard
        public ActionResult Dashboard()
        {
            // Check for success message
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

            int sellerId = (int)Session["UserId"];
            var user = db.Users.Find(sellerId);

            var model = new SellerDashboardViewModel
            {
                Seller = user,
                TotalProducts = db.Products.Count(p => p.Seller_ID == sellerId),
                ActiveProducts = db.Products.Count(p => p.Seller_ID == sellerId && p.IsActive),
                OutOfStockProducts = db.Products.Count(p => p.Seller_ID == sellerId && p.Stock <= 0),
                PendingOrders = db.OrderItems
                    .Include(oi => oi.Order)
                    .Count(oi => oi.Product.Seller_ID == sellerId && oi.Order.Status == "Pending"),
                RecentOrders = db.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Order.User)
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Product.Seller_ID == sellerId)
                    .OrderByDescending(oi => oi.Order.Order_date)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        // GET: Seller/EditProfile
        public ActionResult EditProfile()
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

            int sellerId = (int)Session["UserId"];
            var user = db.Users.Find(sellerId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address
            };

            return View(model);
        }

        // POST: Seller/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditProfileViewModel model)
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                int sellerId = (int)Session["UserId"];
                var user = db.Users.Find(sellerId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update only allowed fields
                user.Name = model.Name;
                user.Phone = model.Phone;
                user.Address = model.Address;

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Dashboard");
            }
            return View(model);
        }

        // GET: Seller/Products
        public ActionResult Products(int? page)
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

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

        // GET: Seller/AddProduct
        public ActionResult AddProduct()
        {
            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                product.Seller_ID = (int)Session["UserId"];
                product.CreatedAt = DateTime.Now;
                product.IsActive = true;

                db.Products.Add(product);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Products");
            }

            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Seller/EditProduct/5
        public ActionResult EditProduct(int id)
        {
            int sellerId = (int)Session["UserId"];
            var product = db.Products.Find(id);

            if (product == null || product.Seller_ID != sellerId)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(product.Id);
                if (existingProduct == null || existingProduct.Seller_ID != (int)Session["UserId"])
                {
                    return HttpNotFound();
                }

                // Update all editable fields
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Image = product.Image;
                existingProduct.IsActive = product.IsActive;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.LastUpdated = DateTime.Now;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Products");
            }

            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public ActionResult SalesReport()
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

            int sellerId = (int)Session["UserId"];
            var sales = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Order.User)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.Seller_ID == sellerId && oi.Order.Status == "Completed")
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new SalesReportViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .ToList();

            return View(sales);
        }

        [SellerAuthorization]
        public ActionResult ExportSalesReportPDF()
        {
            if (Session["UserId"] == null || Session["UserRole"].ToString() != "Seller")
            {
                return RedirectToAction("Login", "Account");
            }

            int sellerId = (int)Session["UserId"];
            var sales = db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Order.User)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.Seller_ID == sellerId && oi.Order.Status == "Completed")
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new SalesReportViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var tableCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Seller info
                var seller = db.Users.Find(sellerId);
                Paragraph sellerInfo = new Paragraph($"Sales Report for: {seller.Name}", FontFactory.GetFont(FontFactory.HELVETICA, 12));
                sellerInfo.SpacingAfter = 5f;
                document.Add(sellerInfo);

                Paragraph dateInfo = new Paragraph($"Generated on: {DateTime.Now.ToString("d MMM yyyy")}", FontFactory.GetFont(FontFactory.HELVETICA, 10));
                dateInfo.SpacingAfter = 15f;
                document.Add(dateInfo);

                // Main title
                Paragraph title = new Paragraph("Sales Report Summary", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Table
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1, 3, 2, 2 });

                // Table Headers
                string[] headers = { "Product ID", "Product Name", "Quantity Sold", "Total Revenue" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, tableHeaderFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        BackgroundColor = BaseColor.LIGHT_GRAY
                    };
                    table.AddCell(cell);
                }

                // Table Data
                decimal grandTotal = 0; 
                foreach (var item in sales)
                {
                    table.AddCell(new Phrase(item.ProductId.ToString(), tableCellFont));
                    table.AddCell(new Phrase(item.ProductName, tableCellFont));
                    table.AddCell(new Phrase(item.TotalQuantity.ToString(), tableCellFont));
                    table.AddCell(new Phrase(item.TotalRevenue.ToString("C2"), tableCellFont));
                    grandTotal += (decimal)item.TotalRevenue; // Explicitly cast TotalRevenue to decimal
                }

                // Grand Total Row
                PdfPCell totalLabelCell = new PdfPCell(new Phrase("GRAND TOTAL", tableHeaderFont))
                {
                    Colspan = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = BaseColor.LIGHT_GRAY
                };
                table.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase(grandTotal.ToString("C2"), tableHeaderFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = BaseColor.LIGHT_GRAY
                };
                table.AddCell(totalValueCell);

                document.Add(table);
                document.Close();

                byte[] fileBytes = ms.ToArray();
                return File(fileBytes, "application/pdf", $"SalesReport_{seller.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }


        [HttpPost]
        public JsonResult ToggleProductStatus(int id)
        {
            var product = db.Products.Find(id);
            if (product == null || product.Seller_ID != (int)Session["UserId"])
            {
                return Json(new { success = false, message = "Product not found" });
            }

            product.IsActive = !product.IsActive;
            product.LastUpdated = DateTime.Now;
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = product.IsActive ? "Active" : "Inactive"
            });
        }

        [SellerAuthorization]
        public ActionResult DeleteProduct(int id)
        {
            int sellerId = (int)Session["UserId"];
            var product = db.Products.Find(id);

            if (product == null || product.Seller_ID != sellerId)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        [SellerAuthorization]
        public ActionResult DeleteProductConfirmed(int id)
        {
            var product = db.Products.Find(id);
            if (product == null || product.Seller_ID != (int)Session["UserId"])
            {
                return HttpNotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Products");
        }

        [SellerAuthorization]
        public ActionResult OrderDetails(int id)
        {
            int sellerId = (int)Session["UserId"];
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .FirstOrDefault(o => o.Id == id);

            if (order == null || !order.OrderItems.Any(oi => oi.Product.Seller_ID == sellerId))
            {
                return HttpNotFound();
            }

            // Filter to only show seller's products
            order.OrderItems = order.OrderItems
                .Where(oi => oi.Product.Seller_ID == sellerId)
                .ToList();

            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}