using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Helpers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    public partial class UserDeprecatedController : BaseController
    {
        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("Impersonate")]
        [AllowAnonymous]
        public async Task<IActionResult> Impersonate(string username)
        {
            var principal = User as ClaimsPrincipal;
            // Pass null for serverAuthOptions - impersonation service uses it as a placeholder
            var accessToken = await _impersonateService.ImpersonateUserAsync(username, null, principal);

            return Ok(new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("RevertImpersonate")]
        [AllowAnonymous]
        public async Task<IActionResult> RevertImpersonate()
        {
            // Pass null for serverAuthOptions - impersonation service uses it as a placeholder
            var accessToken = await _impersonateService.RevertImpersonationAsync(User.GetOriginalUsername(), null);

            return Ok(new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("ImpersonateEnabled")]
        [AllowAnonymous]
        public IActionResult ImpersonateEnabled([FromServices] IConfiguration configuration)
        {
            var key = configuration[WebApiConstants.ClaimUserImpersonation];
            var enabled = key != null && bool.Parse(key);

            return Ok(new { enabled });
        }
    }
}
