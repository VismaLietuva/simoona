using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Api.Controllers.Wall;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.KudosTypes;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;
using WebApi.OutputCache.V2;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers.Kudos
{
    [Authorize]
    [AutoInvalidateCacheOutput]
    public class KudosController : BaseController
    {
        private const int FirstPage = 1;

        private readonly IMapper _mapper;
        private readonly IKudosService _kudosService;
        private readonly IKudosExportService _kudosExportService;
        private readonly IPermissionService _permissionService;

        public KudosController(IMapper mapper, IKudosService kudosService, IKudosExportService kudosExportService, IPermissionService permissionService)
        {
            _mapper = mapper;
            _kudosService = kudosService;
            _kudosExportService = kudosExportService;
            _permissionService = permissionService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public async Task<PagedViewModel<KudosLogViewModel>> GetKudosLogs([FromUri] KudosLogsFilterViewModel filter)
        {
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDTO>(filter);
            SetOrganizationAndUser(filterDto);
            var kudosLogsEntriesDto = await _kudosService.GetKudosLogsAsync(filterDto);
            var kudosLogsViewModel = _mapper.Map<IEnumerable<MainKudosLogDTO>, IEnumerable<KudosLogViewModel>>(kudosLogsEntriesDto.KudosLogs);

            var pagedKudosLogs = new PagedViewModel<KudosLogViewModel>
            {
                PagedList = await kudosLogsViewModel.ToPagedListAsync(FirstPage, BusinessLayerConstants.MaxKudosLogsPerPage),
                ItemCount = kudosLogsEntriesDto.TotalKudosCount,
                PageSize = BusinessLayerConstants.MaxKudosLogsPerPage
            };
            return pagedKudosLogs;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public async Task<IHttpActionResult> GetUserKudosLogs(string userId, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var kudosLogsEntriesDto = await _kudosService.GetUserKudosLogsAsync(userId, page, GetUserAndOrganization().OrganizationId);
            var userKudosLogsViewModel = _mapper.Map<IEnumerable<KudosUserLogDTO>, IEnumerable<KudosUserLogViewModel>>(kudosLogsEntriesDto.KudosLogs);
            var pagedKudosLogs = new PagedViewModel<KudosUserLogViewModel>
            {
                PagedList = await userKudosLogsViewModel.ToPagedListAsync(FirstPage, BusinessLayerConstants.MaxKudosLogsPerPage),
                ItemCount = kudosLogsEntriesDto.TotalKudosCount,
                PageSize = BusinessLayerConstants.MaxKudosLogsPerPage
            };
            return Ok(pagedKudosLogs);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public IEnumerable<KudosTypeViewModel> GetKudosTypesForFilter()
        {
            var types = new List<KudosTypeViewModel>
            {
                new KudosTypeViewModel
                {
                    Hidden = false,
                    Id = 0,
                    Name = BusinessLayerConstants.KudosStatusAllFilter,
                    Value = 0
                }
            };

            foreach (var type in GetKudosTypes())
            {
                types.Add(type);
            }

            return types;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IEnumerable<string> GetKudosStatuses()
        {
            var statuses = new List<string> { BusinessLayerConstants.KudosStatusAllFilter };

            foreach (var status in Enum.GetNames(typeof(KudosStatus)))
            {
                statuses.Add(status);
            }

            return statuses;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IEnumerable<string> GetKudosFilteringTypes()
        {
            var statuses = new List<string> { BusinessLayerConstants.KudosFilteringTypeAllFilter };
            var kudosTypeDto = _kudosService.GetKudosTypes(GetUserAndOrganization());
            return statuses.Concat(kudosTypeDto.Select(s => s.Name));
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IEnumerable<KudosPieChartSliceViewModel> KudosPieChartData(string userId = null)
        {
            userId ??= User.Identity.GetUserId();

            var pieChartDto = _kudosService.GetKudosPieChartData(GetUserAndOrganization().OrganizationId, userId);
            var result = _mapper.Map<IEnumerable<KudosPieChartSliceDto>, IEnumerable<KudosPieChartSliceViewModel>>(pieChartDto);
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public IEnumerable<KudosTypeViewModel> GetKudosTypes()
        {
            var kudosTypeDto = _kudosService.GetKudosTypes(GetUserAndOrganization());
            var kudosTypeViewModel = _mapper.Map<IEnumerable<KudosTypeDTO>, IEnumerable<KudosTypeViewModel>>(kudosTypeDto);

            return kudosTypeViewModel;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public KudosTypeViewModel GetSendKudosType()
        {
            var kudosTypeDto = _kudosService.GetSendKudosType(GetUserAndOrganization());
            return _mapper.Map<KudosTypeDTO, KudosTypeViewModel>(kudosTypeDto);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [InvalidateCacheOutput("GetKudosTypes", typeof(KudosController))]
        [InvalidateCacheOutput("GetKudosTypesForFilter", typeof(KudosController))]
        public async Task<IHttpActionResult> EditType(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                var dto = await _kudosService.GetKudosTypeAsync(id, GetUserAndOrganization());
                var result = _mapper.Map<KudosTypeDTO, KudosTypeViewModel>(dto);

                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [InvalidateCacheOutput("GetKudosTypes", typeof(KudosController))]
        [InvalidateCacheOutput("GetKudosTypesForFilter", typeof(KudosController))]
        public async Task<IHttpActionResult> EditType(KudosTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<KudosTypeViewModel, KudosTypeDTO>(model);
            SetOrganizationAndUser(dto);

            try
            {
                await _kudosService.UpdateKudosTypeAsync(dto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [InvalidateCacheOutput("GetKudosTypes", typeof(KudosController))]
        [InvalidateCacheOutput("GetKudosTypesForFilter", typeof(KudosController))]
        public async Task<IHttpActionResult> CreateType(NewKudosTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<NewKudosTypeViewModel, NewKudosTypeDto>(model);
            SetOrganizationAndUser(dto);

            try
            {
                await _kudosService.CreateKudosType(dto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [InvalidateCacheOutput("GetKudosTypes", typeof(KudosController))]
        [InvalidateCacheOutput("GetKudosTypesForFilter", typeof(KudosController))]
        public async Task<IHttpActionResult> RemoveType(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                await _kudosService.RemoveKudosTypeAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IHttpActionResult GetKudosTypeId(string kudosTypeName)
        {
            var typeId = _kudosService.GetKudosTypeId(kudosTypeName);
            return Ok(new { kudosTypeId = typeId });
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public async Task<IHttpActionResult> AddKudosLog(AddKudosLogViewModel kudosLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Resources.Models.Kudos.Kudos.KudosifyModalError);
            }

            var kudosLogDto = _mapper.Map<AddKudosLogViewModel, AddKudosLogDTO>(kudosLog);
            SetOrganizationAndUser(kudosLogDto);
            try
            {
                if (kudosLog.TotalPointsPerReceiver.HasValue &&
                    await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.Kudos))
                {
                    await _kudosService.AddKudosLogAsync(kudosLogDto, kudosLog.TotalPointsPerReceiver.Value);
                }
                else
                {
                    await _kudosService.AddKudosLogAsync(kudosLogDto);
                }

                return Ok();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult ApproveKudos(int id)
        {
            try
            {
                _kudosService.ApproveKudosAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public IHttpActionResult RejectKudos(KudosRejectViewModel kudosRejectModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var kudosRejectDto = _mapper.Map<KudosRejectViewModel, KudosRejectDTO>(kudosRejectModel);
            SetOrganizationAndUser(kudosRejectDto);

            try
            {
                _kudosService.RejectKudosAsync(kudosRejectDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public async Task<UserKudosViewModel> GetUserKudosInformationById(string id = null)
        {
            id ??= User.Identity.GetUserId();

            var userKudosDto = await _kudosService.GetUserKudosInformationByIdAsync(id, GetUserAndOrganization().OrganizationId);
            var userKudosViewModel = _mapper.Map<UserKudosDTO, UserKudosViewModel>(userKudosDto);

            var monthlyStatistics = await _kudosService.GetMonthlyKudosStatisticsAsync(id);
            userKudosViewModel.SentKudos = monthlyStatistics[0];
            userKudosViewModel.AvailableKudos = monthlyStatistics[1];

            return userKudosViewModel;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IHttpActionResult GetApprovedKudosList(string id = null)
        {
            id ??= User.Identity.GetUserId();

            try
            {
                var userKudosInformationDto = _kudosService.GetApprovedKudosList(id, GetUserAndOrganization().OrganizationId);
                var result = _mapper.Map<IEnumerable<UserKudosInformationDTO>, IEnumerable<UserKudosInformationViewModel>>(userKudosInformationDto);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IEnumerable<WallKudosLogViewModel>> GetLastKudosLogRecords()
        {
            var userAndOrg = GetUserAndOrganization();
            var wallKudosLogsDto = await _kudosService.GetLastKudosLogsForWallAsync(userAndOrg);
            return _mapper.Map<IEnumerable<WallKudosLogDTO>, IEnumerable<WallKudosLogViewModel>>(wallKudosLogsDto);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IEnumerable<KudosBasicDataViewModel>> GetKudosStats(int months, int amount)
        {
            if (months <= 0 || amount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var kudosStatsDto = await _kudosService.GetKudosStatsAsync(months, amount, GetUserAndOrganization().OrganizationId);
            var result = _mapper.Map<IEnumerable<KudosBasicDataDTO>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IEnumerable<KudosListBasicDataViewModel>> GetKudosWidgetStats(int tabOneMonths, int tabOneAmount, int tabTwoMonths, int tabTwoAmount)
        {
            var result = new List<KudosListBasicDataViewModel>();
            if (tabOneMonths <= 0 || tabOneAmount <= 0 || tabTwoMonths <= 0 || tabTwoAmount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            result.Add(await CalculateStatsAsync(tabOneMonths, tabOneAmount));
            result.Add(await CalculateStatsAsync(tabTwoMonths, tabTwoAmount));
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public async Task<IHttpActionResult> GetKudosLogAsExcel([FromUri] KudosLogsFilterViewModel filter)
        {
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDTO>(filter);
            SetOrganizationAndUser(filterDto);
            try
            {
                var stream = new ByteArrayContent(await _kudosExportService.ExportToExcelAsync(filterDto));
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = stream };
                return ResponseMessage(result);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public async Task<IHttpActionResult> GetWelcomeKudos()
        {
            var welcomeKudosDTO = await _kudosService.GetWelcomeKudosAsync();

            var result = _mapper.Map<WelcomeKudosDTO, WelcomeKudosViewModel>(welcomeKudosDTO);

            return Ok(result);
        }

        private async Task<KudosListBasicDataViewModel> CalculateStatsAsync(int months, int amount)
        {
            var kudosStatsDto = await _kudosService.GetKudosStatsAsync(months, amount, GetUserAndOrganization().OrganizationId);
            var stats = _mapper.Map<IEnumerable<KudosBasicDataDTO>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return new KudosListBasicDataViewModel
            {
                Users = stats,
                Months = months
            };
        }
    }
}