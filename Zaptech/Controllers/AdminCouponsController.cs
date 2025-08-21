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
    [RoutePrefix("Admin/Coupons")]
    [AdminLayout]
    [AdminAuthorization]
    public class AdminCouponsController : Controller
    {
        private DB_Conn db = new DB_Conn();

        [Route("")]
        public ActionResult Index(string statusFilter, int? page)
        {
            var coupons = db.Coupons.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "Active";
                coupons = coupons.Where(c => c.IsActive == isActive);
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.StatusFilter = statusFilter;
            return View(coupons.OrderByDescending(c => c.EndDate).ToPagedList(pageNumber, pageSize));
        }

        [Route("Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public ActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.Code = coupon.Code.ToUpper();
                db.Coupons.Add(coupon);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Coupon created successfully!";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        [Route("Edit/{id:int}")]
        public ActionResult Edit(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id:int}")]
        public ActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.Code = coupon.Code.ToUpper();
                db.Entry(coupon).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Coupon updated successfully!";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ToggleStatus/{id:int}")]
        public ActionResult ToggleStatus(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Coupon status changed!";
            }
            return RedirectToAction("Index");
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("Delete/{id:int}")]
        //public ActionResult Delete(int id)
        //{
        //    var coupon = db.Coupons.Find(id);
        //    if (coupon != null)
        //    {
        //        db.Coupons.Remove(coupon);
        //        db.SaveChanges();
        //        TempData["SuccessMessage"] = "Coupon deleted successfully!";
        //    }
        //    return RedirectToAction("Index");
        //}

        [Route("ExportCouponsPDF")]
        public ActionResult ExportCouponsPDF()
        {
            var coupons = db.Coupons.OrderBy(c => c.EndDate).ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var tableCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                Paragraph title = new Paragraph("Coupon List", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Table
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2, 2, 2, 2, 1, 1 });

                // Table Headers
                string[] headers = { "Code", "Discount", "Valid From", "Valid To", "Uses", "Status" };
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
                foreach (var coupon in coupons)
                {
                    table.AddCell(new Phrase(coupon.Code, tableCellFont));
                    table.AddCell(new Phrase(
                        coupon.DiscountType == "Percentage" ? $"{coupon.DiscountValue}%" : $"{coupon.DiscountValue:C}",
                        tableCellFont));
                    table.AddCell(new Phrase(coupon.StartDate.ToString("d MMM yyyy"), tableCellFont));
                    table.AddCell(new Phrase(coupon.EndDate.ToString("d MMM yyyy"), tableCellFont));
                    table.AddCell(new Phrase($"{coupon.CurrentUses}/{(coupon.MaxUses?.ToString() ?? "∞")}", tableCellFont));
                    table.AddCell(new Phrase(coupon.IsActive ? "Active" : "Inactive", tableCellFont));
                }

                document.Add(table);
                document.Close();

                byte[] fileBytes = ms.ToArray();
                return File(fileBytes, "application/pdf", "CouponsList.pdf");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}