using System.Web.Http;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Enable attribute routing
        config.MapHttpAttributeRoutes();

        
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        // Return JSON by default
        config.Formatters.JsonFormatter.SupportedMediaTypes
            .Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));

        // Enable CORS
        config.EnableCors();
    }
}