using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    public class VacationsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVacationService _vacationService;
        private readonly IVacationHistoryService _vacationHistoryService;

        public VacationsController(IMapper mapper, IVacationService vacationService, IVacationHistoryService vacationHistoryService)
        {
            _mapper = mapper;

            _vacationService = vacationService;
            _vacationHistoryService = vacationHistoryService;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Vacation)]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                UnsupportedMediaType();
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var fileContent = provider.Contents[0];

            if (fileContent.Headers.ContentLength >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            var importStatus = _vacationService.UploadVacationReportFileAsync(await fileContent.ReadAsStreamAsync());

            return Ok(importStatus);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IHttpActionResult> AvailableDays()
        {
            var availableDaysDto = await _vacationService.GetAvailableDaysAsync(GetUserAndOrganization());
            var availableDaysViewModel = _mapper.Map<VacationAvailableDaysViewModel>(availableDaysDto);

            return Ok(availableDaysViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IHttpActionResult> GetVacationHistory()
        {
            try
            {
                var vacationDtos = await _vacationHistoryService.GetVacationHistoryAsync(User.Identity.GetUserId());
                var vacationModels = _mapper.Map<VacationDto[], VacationViewModel[]>(vacationDtos);
                return Ok(vacationModels);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
