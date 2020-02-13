using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Exceptions;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Vacations;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Vacations;

namespace Shrooms.Premium.Main.PresentationLayer.API.Controllers
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

            var importStatus = _vacationService.UploadVacationReportFile(await fileContent.ReadAsStreamAsync());

            return Ok(importStatus);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IHttpActionResult> AvailableDays()
        {
            var availableDaysDto = await _vacationService.GetAvailableDays(GetUserAndOrganization());
            var availableDaysViewModel = _mapper.Map<VacationAvailableDaysViewModel>(availableDaysDto);

            return Ok(availableDaysViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IHttpActionResult> GetVacationHistory()
        {
            try
            {
                var vacationDtos = await _vacationHistoryService.GetVacationHistory(User.Identity.GetUserId());
                var vacationModels = _mapper.Map<VacationDTO[], VacationViewModel[]>(vacationDtos);
                return Ok(vacationModels);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}