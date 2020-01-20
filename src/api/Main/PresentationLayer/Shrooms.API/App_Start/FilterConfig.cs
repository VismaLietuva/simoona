using System.Web.Http.Filters;
using System.Web.Mvc;
using Shrooms.API.Filters;
using WebApiAuthorizeAttribute = System.Web.Http.AuthorizeAttribute;

namespace Shrooms.API
{
    public static class FilterConfig
    {
        public static void RegisterGlobalMvcFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ApplicationInsightsHandleErrorAttribute());
        }

        public static void RegisterGlobalWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new WebApiAuthorizeAttribute());
            filters.Add(new OrganizationExceptionFilterAttribute());
            filters.Add(new OrganizationValidationFilterAttribute());
        }
    }
}