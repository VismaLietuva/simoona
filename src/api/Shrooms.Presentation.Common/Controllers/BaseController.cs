using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public StatusCodeResult Forbidden()
        {
            return StatusCode(403);
        }

        [NonAction]
        public StatusCodeResult UnsupportedMediaType()
        {
            return StatusCode(415);
        }

        [NonAction]
        public IActionResult BadRequestWithError(ValidationException ex)
        {
            return BadRequest(new { ErrorCode = ex.ErrorCode, ErrorMessage = ex.ErrorMessage });
        }

        [NonAction]
        public UserAndOrganizationDto GetUserAndOrganization()
        {
            return User.Identity.GetUserAndOrganization();
        }

        [NonAction]
        public UserAndOrganizationHubDto GetUserAndOrganizationHub()
        {
            return new UserAndOrganizationHubDto
            {
                OrganizationId = User.Identity.GetOrganizationId(),
                UserId = User.Identity.GetUserId(),
                OrganizationName = User.Identity.GetOrganizationName()
            };
        }

        [NonAction]
        public int GetOrganizationId()
        {
            return User.Identity.GetOrganizationId();
        }

        [NonAction]
        public string GetOrganizationName()
        {
            return User.Identity.GetOrganizationName();
        }

        [NonAction]
        public void SetOrganizationAndUser(UserAndOrganizationDto obj)
        {
            obj.OrganizationId = User.Identity.GetOrganizationId();
            obj.UserId = User.Identity.GetUserId();
        }
    }
}
