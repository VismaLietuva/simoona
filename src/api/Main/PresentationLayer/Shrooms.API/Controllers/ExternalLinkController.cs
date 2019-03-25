using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Controllers.WebApi;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Domain.Services.ExternalLinks;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.WebViewModels.Models.ExternalLink;
using WebApi.OutputCache.V2;

namespace Shrooms.API.Controllers
{
    [Authorize]
    [RoutePrefix("ExternalLink")]
    [AutoInvalidateCacheOutput]
    public class ExternalLinkController : BaseController
    {
        private readonly IExternalLinkService _externalLinkService;
        private readonly IMapper _mapper;

        public ExternalLinkController(IExternalLinkService externalLinkService, IMapper mapper)
        {
            _externalLinkService = externalLinkService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAuthorize(Permission = BasicPermissions.ExternalLink)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneHour)]
        public IHttpActionResult GetAll()
        {
            var externalLinks = _externalLinkService.GetAll(GetUserAndOrganization().OrganizationId);
            var externalLinksViewModel = _mapper.Map<IEnumerable<ExternalLinkDTO>, IEnumerable<ExternalLinkViewModel>>(externalLinks);
            return Ok(externalLinksViewModel);
        }

        [HttpPost]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ExternalLink)]
        public IHttpActionResult UpdateLinks(AddEditDeleteExternalLinkViewModel updateLinksViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateLinksDto = _mapper.Map<AddEditDeleteExternalLinkViewModel, AddEditDeleteExternalLinkDTO>(updateLinksViewModel);
            SetOrganizationAndUser(updateLinksDto);

            try
            {
                _externalLinkService.UpdateLinks(updateLinksDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((ExternalLinkController t) => t.GetAll()));

            return Ok();
        }
    }
}
