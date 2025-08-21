using System.Web.Mvc;

namespace Zaptech.Filters
{
    public class CustomerAuthorization : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["UserId"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else if (filterContext.HttpContext.Session["UserRole"].ToString() != "Customer")
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }
}