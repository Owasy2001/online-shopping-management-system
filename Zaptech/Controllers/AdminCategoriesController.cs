using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Models;
using PagedList;
using Zaptech.Filters;

namespace Zaptech.Controllers
{
    [RoutePrefix("AdminCategories/Categories")]
    [AdminLayoutAttribute]
    public class AdminCategoriesController : Controller
    {
        private DB_Conn db = new DB_Conn();

        [Route("")]

        public ActionResult Index(string searchString, string statusFilter, int? page)
        {
            ViewBag.CurrentFilter = searchString;
            ViewBag.StatusFilter = statusFilter;

            var categories = db.Categorys.Include(c => c.Products).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                categories = categories.Where(c => c.Status == statusFilter);
            }

            ViewBag.StatusList = new SelectList(new[] { "Active", "Inactive", "Archived" });

            return View(categories.OrderBy(c => c.Name).ToPagedList(page ?? 1, 10));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form)
        {
            var category = new Category
            {
                Name = form["Name"],
                Status = form["Status"]
            };

            // Handle file upload
            var file = Request.Files["ImageFile"];
            if (file != null && file.ContentLength > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string path = Path.Combine(Server.MapPath("~/Content/CategoryImages"), fileName);
                file.SaveAs(path);
                category.GetType().GetProperty("ImagePath")?.SetValue(category, "/Content/CategoryImages/" + fileName);
            }

            db.Categorys.Add(category);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var category = db.Categorys.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, FormCollection form)
        {
            var category = db.Categorys.Find(id);
            if (category == null) return HttpNotFound();

            category.Name = form["Name"];
            category.Status = form["Status"];

            var file = Request.Files["ImageFile"];
            if (file != null && file.ContentLength > 0)
            {
                string oldPath = Server.MapPath((string)category.GetType().GetProperty("ImagePath")?.GetValue(category));
                if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }

                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string path = Path.Combine(Server.MapPath("~/Content/CategoryImages"), fileName);
                file.SaveAs(path);
                category.GetType().GetProperty("ImagePath")?.SetValue(category, "/Content/CategoryImages/" + fileName);
            }

            db.Entry(category).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var category = db.Categorys.Include(c => c.Products).FirstOrDefault(c => c.Id == id);
            if (category == null) return HttpNotFound();

            if (category.Products.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category with associated products.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categorys.Include(c => c.Products).FirstOrDefault(c => c.Id == id);
            if (category == null) return HttpNotFound();

            if (category.Products.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category with associated products.";
                return RedirectToAction("Index");
            }

            string imagePath = (string)category.GetType().GetProperty("ImagePath")?.GetValue(category);
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = Server.MapPath(imagePath);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            db.Categorys.Remove(category);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
