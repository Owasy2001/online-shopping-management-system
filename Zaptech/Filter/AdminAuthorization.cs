// AdminAuthorization.cs in Filters folder
//using System.Web.Mvc;
//using Zaptech.Models;

//namespace Zaptech.Filters
//{
//    public class AdminAuthorization : ActionFilterAttribute, IAuthorizationFilter
//    {
//        public void OnAuthorization(AuthorizationContext filterContext)
//        {
//            if (filterContext.HttpContext.Session["UserId"] == null)
//            {
//                filterContext.Result = new RedirectResult("~/Account/Login");
//            }
//            else if ((UserRole)filterContext.HttpContext.Session["UserRole"] != UserRole.Admin)
//            {
//                filterContext.Result = new RedirectResult("~/Account/Login");
//            }
//        }
//    }
//}

// Filters/AdminAuthorization.cs
using System.Web.Mvc;

public class AdminAuthorization : AuthorizeAttribute
{
    protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
    {
        return httpContext.Session["UserRole"]?.ToString() == "Admin";
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.Result = new RedirectResult("~/Account/Login");
    }
}