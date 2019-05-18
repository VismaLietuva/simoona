using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Vacations;
using Shrooms.Domain.Services.Vacations;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    public class VacationsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVacationService _vacationService;
        private readonly IVacationHistoryService _vacationHistoryService;

        public VacationsController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IVacationService vacationService,
            IVacationHistoryService vacationHistoryService)
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

            if (fileContent.Headers.ContentLength >= ConstWebApi.MaximumPictureSizeInBytes)
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