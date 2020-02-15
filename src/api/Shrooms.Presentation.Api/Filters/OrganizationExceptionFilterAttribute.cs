using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Shrooms.Domain.Exceptions.Exceptions.Organization;

namespace Shrooms.Presentation.Api.Filters
{
    public class OrganizationExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is InvalidOrganizationException)
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Invalid organization"
                };
                actionExecutedContext.Response = response;
            }
        }
    }
}