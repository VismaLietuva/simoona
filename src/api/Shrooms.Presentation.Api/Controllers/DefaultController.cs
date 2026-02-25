using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    [SkipOrganizationValidationFilter]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return Ok("API is up and running");
        }
    }
}