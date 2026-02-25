using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shrooms.Domain.Exceptions.Exceptions.Organization;

namespace Shrooms.Presentation.Api.Filters
{
    public class OrganizationExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is InvalidOrganizationException)
            {
                context.Result = new BadRequestObjectResult("Invalid organization");
                context.ExceptionHandled = true;
            }
        }
    }
}