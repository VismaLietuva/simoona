using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.ExternalLinks;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace Shrooms.Presentation.Api.Controllers
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
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IHttpActionResult> GetAll()
        {
            var externalLinks = await _externalLinkService.GetAllAsync(GetUserAndOrganization().OrganizationId);
            var externalLinksViewModel = _mapper.Map<IEnumerable<ExternalLinkDto>, IEnumerable<ExternalLinkViewModel>>(externalLinks);
            return Ok(externalLinksViewModel);
        }

        [HttpGet]
        [Route("{id}")]
        [PermissionAuthorize(Permission = BasicPermissions.ExternalLink)]
        public async Task<IHttpActionResult> GetExternalLink(int id)
        {
            var externalLink = await _externalLinkService.GetAsync(id, GetUserAndOrganization());

            if (externalLink == null)
            {
                return NotFound();
            }

            var externalLinkDto = _mapper.Map<ExternalLinkDto, ExternalLinkViewModel>(externalLink);

            return Ok(externalLinkDto);
        }

        [HttpPost]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ExternalLink)]
        [InvalidateCacheOutput(nameof(GetAll))]
        public async Task<IHttpActionResult> UpdateLinks(ManageExternalLinkViewModel manageLinksViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateLinksDto = _mapper.Map<ManageExternalLinkViewModel, ManageExternalLinkDto>(manageLinksViewModel);
            SetOrganizationAndUser(updateLinksDto);

            try
            {
                await _externalLinkService.UpdateLinksAsync(updateLinksDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
