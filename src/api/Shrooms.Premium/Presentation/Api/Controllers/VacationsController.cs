using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;

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

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Vacation)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.Length >= WebApiConstants.MaximumPictureSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            using var stream = file.OpenReadStream();
            var importStatus = await _vacationService.UploadVacationReportFileAsync(stream);

            return Ok(importStatus);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IActionResult> AvailableDays()
        {
            var availableDaysDto = await _vacationService.GetAvailableDaysAsync(GetUserAndOrganization());
            var availableDaysViewModel = _mapper.Map<VacationAvailableDaysViewModel>(availableDaysDto);

            return Ok(availableDaysViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        public async Task<IActionResult> GetVacationHistory()
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
