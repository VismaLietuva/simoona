using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Domain.Exceptions.Exceptions.Organization;

namespace Shrooms.Presentation.Api.Filters
{
    public class OrganizationValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var ignoreAttribute =
                actionContext.ActionDescriptor.GetCustomAttributes<SkipOrganizationValidationFilterAttribute>(false).Any() ||
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<SkipOrganizationValidationFilterAttribute>(false).Any();
            if (ignoreAttribute)
            {
                return;
            }

            var tenant = actionContext.Request.GetRequestedTenant();
            if (string.IsNullOrEmpty(tenant))
            {
                throw new InvalidOrganizationException();
            }
        }
    }
}