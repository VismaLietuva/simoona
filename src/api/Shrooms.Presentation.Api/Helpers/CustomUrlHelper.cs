using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Shrooms.Presentation.Api.Helpers
{
    public static class CustomUrlHelper
    {
        public static string RouteFromController(this IUrlHelper helper, string route, string controllerName, object routeData)
        {
            var urlPath = helper.RouteUrl(new UrlRouteContext { RouteName = route, Values = routeData }) ?? string.Empty;

            var pos = urlPath.IndexOf(controllerName ?? string.Empty, StringComparison.Ordinal);

            return pos < 2 ? urlPath : urlPath.Substring(pos - 1);
        }
    }
}