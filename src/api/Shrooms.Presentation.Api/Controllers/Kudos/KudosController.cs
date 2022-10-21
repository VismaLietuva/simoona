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
using Shrooms.Domain.Services.FilterPresets;
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

        public KudosController(
            IMapper mapper,
            IKudosService kudosService,
            IKudosExportService kudosExportService,
            IPermissionService permissionService)
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
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDto>(filter);
            SetOrganizationAndUser(filterDto);
            var kudosLogsEntriesDto = await _kudosService.GetKudosLogsAsync(filterDto);
            var kudosLogsViewModel = _mapper.Map<IEnumerable<MainKudosLogDto>, IEnumerable<KudosLogViewModel>>(kudosLogsEntriesDto.KudosLogs);

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
            var userKudosLogsViewModel = _mapper.Map<IEnumerable<KudosUserLogDto>, IEnumerable<KudosUserLogViewModel>>(kudosLogsEntriesDto.KudosLogs);
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
        public async Task<IEnumerable<KudosTypeViewModel>> GetKudosTypesForFilter()
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

            foreach (var type in await GetKudosTypes())
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
        public async Task<IEnumerable<string>> GetKudosFilteringTypes()
        {
            var statuses = new List<string> { BusinessLayerConstants.KudosFilteringTypeAllFilter };
            var kudosTypeDto = await _kudosService.GetKudosTypesAsync(GetUserAndOrganization());
            return statuses.Concat(kudosTypeDto.Select(s => s.Name));
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public async Task<IEnumerable<KudosPieChartSliceViewModel>> KudosPieChartData(string userId = null)
        {
            userId ??= User.Identity.GetUserId();

            var pieChartDto = await _kudosService.GetKudosPieChartDataAsync(GetUserAndOrganization().OrganizationId, userId);
            var result = _mapper.Map<IEnumerable<KudosPieChartSliceDto>, IEnumerable<KudosPieChartSliceViewModel>>(pieChartDto);
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IEnumerable<KudosTypeViewModel>> GetKudosTypes()
        {
            var kudosTypeDto = await _kudosService.GetKudosTypesAsync(GetUserAndOrganization());
            var kudosTypeViewModel = _mapper.Map<IEnumerable<KudosTypeDto>, IEnumerable<KudosTypeViewModel>>(kudosTypeDto);

            return kudosTypeViewModel;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public async Task<KudosTypeViewModel> GetSendKudosType()
        {
            var kudosTypeDto = await _kudosService.GetSendKudosTypeAsync(GetUserAndOrganization());
            return _mapper.Map<KudosTypeDto, KudosTypeViewModel>(kudosTypeDto);
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
                var result = _mapper.Map<KudosTypeDto, KudosTypeViewModel>(dto);

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

            var dto = _mapper.Map<KudosTypeViewModel, KudosTypeDto>(model);
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
                await _kudosService.CreateKudosTypeAsync(dto);
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
        public async Task<IHttpActionResult> GetKudosTypeId(string kudosTypeName)
        {
            var typeId = await _kudosService.GetKudosTypeIdAsync(kudosTypeName);
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

            var kudosLogDto = _mapper.Map<AddKudosLogViewModel, AddKudosLogDto>(kudosLog);
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
        public async Task<IHttpActionResult> ApproveKudos(int id)
        {
            try
            {
                await _kudosService.ApproveKudosAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public async Task<IHttpActionResult> RejectKudos(KudosRejectViewModel kudosRejectModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var kudosRejectDto = _mapper.Map<KudosRejectViewModel, KudosRejectDto>(kudosRejectModel);
            SetOrganizationAndUser(kudosRejectDto);

            try
            {
                await _kudosService.RejectKudosAsync(kudosRejectDto);
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
            var userKudosViewModel = _mapper.Map<UserKudosDto, UserKudosViewModel>(userKudosDto);

            var monthlyStatistics = await _kudosService.GetMonthlyKudosStatisticsAsync(id);
            userKudosViewModel.SentKudos = monthlyStatistics[0];
            userKudosViewModel.AvailableKudos = monthlyStatistics[1];

            return userKudosViewModel;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public async Task<IHttpActionResult> GetApprovedKudosList(string id = null)
        {
            id ??= User.Identity.GetUserId();

            try
            {
                var userKudosInformationDto = await _kudosService.GetApprovedKudosListAsync(id, GetUserAndOrganization().OrganizationId);
                var result = _mapper.Map<IEnumerable<UserKudosInformationDto>, IEnumerable<UserKudosInformationViewModel>>(userKudosInformationDto);
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
            return _mapper.Map<IEnumerable<WallKudosLogDto>, IEnumerable<WallKudosLogViewModel>>(wallKudosLogsDto);
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
            var result = _mapper.Map<IEnumerable<KudosBasicDataDto>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
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
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDto>(filter);
            SetOrganizationAndUser(filterDto);
            try
            {
                var content = await _kudosExportService.ExportToExcelAsync(filterDto);

                var result = new HttpResponseMessage(HttpStatusCode.OK) 
                {
                    Content = content 
                };

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
            var welcomeKudos = await _kudosService.GetWelcomeKudosAsync();

            var result = _mapper.Map<WelcomeKudosDto, WelcomeKudosViewModel>(welcomeKudos);

            return Ok(result);
        }

        private async Task<KudosListBasicDataViewModel> CalculateStatsAsync(int months, int amount)
        {
            var kudosStatsDto = await _kudosService.GetKudosStatsAsync(months, amount, GetUserAndOrganization().OrganizationId);
            var stats = _mapper.Map<IEnumerable<KudosBasicDataDto>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return new KudosListBasicDataViewModel
            {
                Users = stats,
                Months = months
            };
        }
    }
}