using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Filters;
using Shrooms.API.Helpers;
using Shrooms.Constants.WebApi;

namespace Shrooms.API.Controllers.WebApi
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
            var access_token = await _impesonateService.ImpersonateUserAsync(username, Startup.OAuthServerOptions, principal);

            return Request.CreateResponse(HttpStatusCode.OK, new { access_token });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("RevertImpersonate")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> RevertImpersonate()
        {
            var access_token = await _impesonateService.RevertImpersonationAsync(User.GetOriginalUsername(), Startup.OAuthServerOptions);

            return Request.CreateResponse(HttpStatusCode.OK, new { access_token });
        }

        [HttpGet]
        [FeatureToggle(Infrastructure.FeatureToggle.Features.Impersonation)]
        [Route("ImpersonateEnabled")]
        [AllowAnonymous]
        public HttpResponseMessage ImpersonateEnabled()
        {
            var key = System.Configuration.ConfigurationManager.AppSettings[ConstWebApi.ClaimUserImpersonation];

            var enabled = key != null ? bool.Parse(key) : false;

            return Request.CreateResponse(HttpStatusCode.OK, new { enabled });
        }
    }
}