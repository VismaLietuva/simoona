using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        public StatusCodeResult Forbidden()
        {
            return StatusCode(403);
        }

        public StatusCodeResult UnsupportedMediaType()
        {
            return StatusCode(415);
        }

        public IActionResult BadRequestWithError(ValidationException ex)
        {
            return BadRequest(new { ErrorCode = ex.ErrorCode, ErrorMessage = ex.ErrorMessage });
        }

        public UserAndOrganizationDto GetUserAndOrganization()
        {
            return User.Identity.GetUserAndOrganization();
        }

        public UserAndOrganizationHubDto GetUserAndOrganizationHub()
        {
            return new UserAndOrganizationHubDto
            {
                OrganizationId = User.Identity.GetOrganizationId(),
                UserId = User.Identity.GetUserId(),
                OrganizationName = User.Identity.GetOrganizationName()
            };
        }

        public int GetOrganizationId()
        {
            return User.Identity.GetOrganizationId();
        }

        public string GetOrganizationName()
        {
            return User.Identity.GetOrganizationName();
        }

        public void SetOrganizationAndUser(UserAndOrganizationDto obj)
        {
            obj.OrganizationId = User.Identity.GetOrganizationId();
            obj.UserId = User.Identity.GetUserId();
        }
    }
}
