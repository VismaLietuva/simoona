using System.Web.Http;
using System.Web.Http.Filters;
using Shrooms.API.Filters;

namespace Shrooms.API
{
    public class FilterConfig
    {
        public static void RegisterGlobalWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new AuthorizeAttribute());
            filters.Add(new OrganizationExceptionFilterAttribute());
            filters.Add(new OrganizationValidationFilterAttribute());
        }
    }
}