using System.Net;
using System.Net.Http;
using System.Web.Http;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    [SkipOrganizationValidationFilter]
    public class DefaultController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Index()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "API is up and running");
        }
    }
}