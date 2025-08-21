using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Filters;
using Zaptech.Models;
using PagedList;

namespace Zaptech.Controllers
{
    [RoutePrefix("Admin/Products")]
    [AdminLayout]
    [AdminAuthorization]
    public class AdminProductsController : Controller
    {
        private DB_Conn db = new DB_Conn();

        [Route("")]  
        // GET: /Admin/Products
        public ActionResult Index(string searchString, int? categoryId, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";

            var products = db.Products.Include(p => p.Category);

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "Price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name");
            ViewBag.SearchString = searchString;

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        [Route("Details/{id:int}")]
        public ActionResult Details(int id)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        [Route("Create")]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.CreatedBy = User.Identity.Name;
                product.Seller_ID = 1;

                db.Products.Add(product);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Route("Edit/{id:int}")]
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit/{id:int}")]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                product.LastUpdated = DateTime.Now;
                product.LastModifiedBy = User.Identity.Name;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categorys, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Route("Delete/{id:int}")]
        public ActionResult Delete(int id)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Delete/{id:int}")]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
