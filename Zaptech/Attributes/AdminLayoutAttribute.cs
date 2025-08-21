// Attributes/AdminLayoutAttribute.cs
using System.Web.Mvc;

public class AdminLayoutAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var user = filterContext.HttpContext.Session["UserRole"];
        if (user?.ToString() == "Admin")
        {
            filterContext.Controller.ViewBag.Layout = "~/Views/Shared/_AdminLayout.cshtml";
        }
        else
        {
            filterContext.Controller.ViewBag.Layout = "~/Views/Shared/_Layout.cshtml";
        }

        base.OnActionExecuting(filterContext);
    }
}