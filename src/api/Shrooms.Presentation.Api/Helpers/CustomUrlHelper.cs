using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Helpers
{
    public static class CustomUrlHelper
    {
        public static string RouteFromController(this IUrlHelper helper, string route, string controllerName, object routeData)
        {
            return helper.RouteUrl(route, routeData) ?? string.Empty;
        }
    }
}