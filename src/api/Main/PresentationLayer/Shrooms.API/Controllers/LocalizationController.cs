using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using Shrooms.API.Filters;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Resources;

namespace Shrooms.API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [AllowAnonymous]
    public class LocalizationController : ApiController
    {
        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Localization)]
        public object GetResource(string resource, string language)
        {
            return ResourceUtilities.GetResource(resource, language);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Localization)]
        public IEnumerable<object> GetResources([FromUri]string[] resources, string language)
        {
            foreach (var resource in resources)
            {
                yield return ResourceUtilities.GetResource(resource, language);
            }
        }
    }
}
