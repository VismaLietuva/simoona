using System.Web.Http;

namespace Shrooms.Presentation.Api
{
    public static class RouteConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.IgnoreRoute("elmah", "{resource}.axd/{*pathInfo}");
            config.Routes.IgnoreRoute("storage", "storage/{*pathInfo}");

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { controller = "Default", action = "Index", id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "Errors",
                routeTemplate: "Error/{action}/",
                defaults: new { action = "NotFound" });
        }
    }
}