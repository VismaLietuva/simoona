using System.Web.Http.Routing;

namespace Shrooms.API.Helpers
{
    public static class CustomUrlHelper
    {
        public static string RouteFromController(this UrlHelper helper, string route, string controllerName, object routeData)
        {
            var urlPath = helper.Route(route, routeData);

            var pos = urlPath.IndexOf(controllerName ?? "");

            return pos < 2 ? urlPath : urlPath.Substring(pos - 1);
        }
    }
}