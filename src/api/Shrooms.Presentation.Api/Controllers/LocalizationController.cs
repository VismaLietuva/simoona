using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Resources;
using System.Collections.Generic;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    public class LocalizationController : ControllerBase
    {
        [HttpGet]
        [Route("Localization/GetResource")]
        [PermissionAuthorize(Permission = BasicPermissions.Localization)]
        public object GetResource(string resource, string language)
        {
            return ResourceUtilities.GetResource(resource, language);
        }

        [HttpGet]
        [Route("Localization/GetResources")]
        [PermissionAuthorize(Permission = BasicPermissions.Localization)]
        public IEnumerable<object> GetResources([FromQuery] string[] resources, string language)
        {
            foreach (var resource in resources)
            {
                yield return ResourceUtilities.GetResource(resource, language);
            }
        }
    }
}
