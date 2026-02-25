using Microsoft.AspNetCore.Mvc.Filters;
using Shrooms.Domain.Exceptions.Exceptions.Organization;
using Shrooms.Presentation.Common.Helpers;
using System.Linq;

namespace Shrooms.Presentation.Api.Filters
{
    public class OrganizationValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ignoreAttribute =
                context.ActionDescriptor.EndpointMetadata.OfType<SkipOrganizationValidationFilterAttribute>().Any();
            if (ignoreAttribute)
            {
                return;
            }

            var tenant = context.HttpContext.GetRequestedTenant();
            if (string.IsNullOrEmpty(tenant))
            {
                throw new InvalidOrganizationException();
            }
        }
    }
}