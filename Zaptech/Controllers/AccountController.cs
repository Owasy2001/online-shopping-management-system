using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Zaptech.Context;
using Zaptech.Models;

namespace Zaptech.Controllers
{
    public class AccountController : Controller
    {
        private DB_Conn db = new DB_Conn();

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // Store user details in session
                Session["UserId"] = user.Id;
                Session["UserName"] = user.Name;
                Session["UserEmail"] = user.Email;
                Session["UserRole"] = user.Role; // Save role to session

               

                if (user.Role == UserRole.Customer)
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
                else if (user.Role == UserRole.Seller)
                {
                    return RedirectToAction("Dashboard", "Seller");
                }
                else if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            // If login failed, show alert
            ViewBag.LoginFailed = "Login Failed. Invalid email or password.";
            return View();
        }


        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

       


        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Prevent registration with Admin role by any means
                if (user.Role == UserRole.Admin)
                {
                    ModelState.AddModelError("Role", "Invalid role selected.");
                    // Repopulate dropdown
                    var rolesForDropdown = new List<SelectListItem>
            {
                new SelectListItem { Text = "Customer", Value = ((int)UserRole.Customer).ToString() },
                new SelectListItem { Text = "Seller", Value = ((int)UserRole.Seller).ToString() }
            };
                    ViewBag.RoleList = rolesForDropdown;
                    return View(user);
                }

                // Check if email exists
                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    var rolesForDropdown = new List<SelectListItem>
            {
                new SelectListItem { Text = "Customer", Value = ((int)UserRole.Customer).ToString() },
                new SelectListItem { Text = "Seller", Value = ((int)UserRole.Seller).ToString() }
            };
                    ViewBag.RoleList = rolesForDropdown;
                    return View(user);
                }

                // Save user to DB
                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            // If model invalid, repopulate dropdown and return view
            var rolesList = new List<SelectListItem>
    {
        new SelectListItem { Text = "Customer", Value = ((int)UserRole.Customer).ToString() },
        new SelectListItem { Text = "Seller", Value = ((int)UserRole.Seller).ToString() }
    };
            ViewBag.RoleList = rolesList;

            return View(user);
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Clear session when logging out
            Session.Clear();
            return RedirectToAction("Index", "CustomerHome"); // Redirect to home after logout
        }
    }
}
