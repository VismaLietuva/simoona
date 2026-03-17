using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Infrastructure.FeatureToggle;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    public partial class UserDeprecatedController : BaseController
    {
        [HttpGet]
        [FeatureToggle(Features.Impersonation)]
        [Route("Impersonate")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IActionResult> Impersonate(string username)
        {
            var principal = User as ClaimsPrincipal;
            var accessToken = await _impersonateService.ImpersonateUserAsync(username, principal);

            return Ok(new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Features.Impersonation)]
        [Route("RevertImpersonate")]
        [Authorize]
        public async Task<IActionResult> RevertImpersonate()
        {
            var accessToken = await _impersonateService.RevertImpersonationAsync(User.GetOriginalUsername());

            return Ok(new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Features.Impersonation)]
        [Route("ImpersonateEnabled")]
        [AllowAnonymous]
        public IActionResult ImpersonateEnabled([FromServices] IFeatureConfiguration featureConfiguration)
        {
            var enabled = featureConfiguration.IsAvailable(Features.Impersonation);

            return Ok(new { enabled });
        }
    }
}
