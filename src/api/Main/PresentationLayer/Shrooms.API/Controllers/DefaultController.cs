using System.Net;
using System.Net.Http;
using System.Web.Http;
using Shrooms.API.Filters;

namespace Shrooms.API.Controllers
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