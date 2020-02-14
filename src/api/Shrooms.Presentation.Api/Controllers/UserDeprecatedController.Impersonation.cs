using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Presentation.Api.Controllers
{
    public partial class UserDeprecatedController : BaseController
    {
        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("Impersonate")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Impersonate(string username)
        {
            var principal = User as ClaimsPrincipal;
            var accessToken = await _impersonateService.ImpersonateUserAsync(username, Startup.OAuthServerOptions, principal);

            return Request.CreateResponse(HttpStatusCode.OK, new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("RevertImpersonate")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> RevertImpersonate()
        {
            var accessToken = await _impersonateService.RevertImpersonationAsync(User.GetOriginalUsername(), Startup.OAuthServerOptions);

            return Request.CreateResponse(HttpStatusCode.OK, new { access_token = accessToken });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("ImpersonateEnabled")]
        [AllowAnonymous]
        public HttpResponseMessage ImpersonateEnabled()
        {
            var key = System.Configuration.ConfigurationManager.AppSettings[WebApiConstants.ClaimUserImpersonation];
            var enabled = key != null && bool.Parse(key);

            return Request.CreateResponse(HttpStatusCode.OK, new { enabled });
        }
    }
}