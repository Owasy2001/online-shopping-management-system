using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zaptech.Context;

namespace Zaptech.Controllers
{
    public class CustomerContactController : Controller
    {
        private DB_Conn db = new DB_Conn();

        // GET: CustomerContact
        public ActionResult Index()
        {
            return View();
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