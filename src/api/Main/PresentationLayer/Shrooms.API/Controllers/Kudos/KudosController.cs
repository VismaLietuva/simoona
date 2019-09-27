using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Kudos;
using Shrooms.WebViewModels.Models.KudosTypes;
using Shrooms.WebViewModels.Models.Users.Kudos;
using WebApi.OutputCache.V2;

namespace Shrooms.API.Controllers.Kudos
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
        public PagedViewModel<KudosLogViewModel> GetKudosLogs([FromUri] KudosLogsFilterViewModel filter)
        {
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDTO>(filter);
            SetOrganizationAndUser(filterDto);
            var kudosLogsEntriesDto = _kudosService.GetKudosLogs(filterDto);
            var kudosLogsViewModel = _mapper.Map<IEnumerable<MainKudosLogDTO>, IEnumerable<KudosLogViewModel>>(kudosLogsEntriesDto.KudosLogs);
            var pagedKudosLogs = new PagedViewModel<KudosLogViewModel>
            {
                PagedList = kudosLogsViewModel.ToPagedList(FirstPage, ConstBusinessLayer.MaxKudosLogsPerPage),
                ItemCount = kudosLogsEntriesDto.TotalKudosCount,
                PageSize = ConstBusinessLayer.MaxKudosLogsPerPage
            };
            return pagedKudosLogs;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IHttpActionResult GetUserKudosLogs(string userId, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var kudosLogsEntriesDto = _kudosService.GetUserKudosLogs(userId, page, GetUserAndOrganization().OrganizationId);
            var userKudosLogsViewModel = _mapper.Map<IEnumerable<KudosUserLogDTO>, IEnumerable<KudosUserLogViewModel>>(kudosLogsEntriesDto.KudosLogs);
            var pagedKudosLogs = new PagedViewModel<KudosUserLogViewModel>
            {
                PagedList = userKudosLogsViewModel.ToPagedList(FirstPage, ConstBusinessLayer.MaxKudosLogsPerPage),
                ItemCount = kudosLogsEntriesDto.TotalKudosCount,
                PageSize = ConstBusinessLayer.MaxKudosLogsPerPage
            };
            return Ok(pagedKudosLogs);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneDay)]
        public IEnumerable<KudosTypeViewModel> GetKudosTypesForFilter()
        {
            var types = new List<KudosTypeViewModel>()
            {
                new KudosTypeViewModel()
                {
                    Hidden = false,
                    Id = 0,
                    Name = ConstBusinessLayer.KudosStatusAllFilter,
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
            var statuses = new List<string> { ConstBusinessLayer.KudosStatusAllFilter };

            foreach (var status in Enum.GetNames(typeof(KudosStatus)))
            {
                statuses.Add(status);
            }

            return statuses;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IEnumerable<string> GetKudosFilteringTypes()
        {
            var statuses = new List<string> { ConstBusinessLayer.KudosFilteringTypeAllFilter };
            var kudosTypeDto = _kudosService.GetKudosTypes(GetUserAndOrganization());
            return statuses.Concat(kudosTypeDto.Select(s => s.Name));
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IEnumerable<KudosPieChartSliceViewModel> KudosPieChartData(string userId = null)
        {
            if (userId == null)
            {
                userId = User.Identity.GetUserId();
            }

            var pieChartDto = _kudosService.GetKudosPieChartData(GetUserAndOrganization().OrganizationId, userId);
            var result = _mapper.Map<IEnumerable<KudosPieChartSliceDto>, IEnumerable<KudosPieChartSliceViewModel>>(pieChartDto);
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneDay)]
        public IEnumerable<KudosTypeViewModel> GetKudosTypes()
        {
            var kudosTypeDto = _kudosService.GetKudosTypes(GetUserAndOrganization());
            var kudosTypeViewModel = _mapper.Map<IEnumerable<KudosTypeDTO>, IEnumerable<KudosTypeViewModel>>(kudosTypeDto);

            return kudosTypeViewModel;
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
                var dto = await _kudosService.GetKudosType(id, GetUserAndOrganization());
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

            try
            {
                await _kudosService.UpdateKudosType(dto);
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
                await _kudosService.RemoveKudosType(id, GetUserAndOrganization());
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

        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public IEnumerable<UserKudosAutocompleteViewModel> GetUsersForAutocomplete(string s)
        {
            var userKudosAutoCompleteDto = _kudosService.GetUsersForAutocomplete(s);
            var result = _mapper.Map<IEnumerable<UserKudosAutocompleteDTO>, IEnumerable<UserKudosAutocompleteViewModel>>(userKudosAutoCompleteDto);
            return result;
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult AddKudosLog(AddKudosLogViewModel kudosLog)
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
                    _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.Kudos))
                {
                    _kudosService.AddKudosLog(kudosLogDto, kudosLog.TotalPointsPerReceiver.Value);
                }
                else
                {
                    _kudosService.AddKudosLog(kudosLogDto);
                }

                return Ok();
            }
            catch (UnauthorizedException) { return Unauthorized(); }
            catch (ValidationException e) { return BadRequestWithError(e); }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult ApproveKudos(int id)
        {
            try
            {
                _kudosService.ApproveKudos(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e) { return BadRequestWithError(e); }
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
                _kudosService.RejectKudos(kudosRejectDto);
                return Ok();
            }
            catch (ValidationException e) { return BadRequestWithError(e); }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public UserKudosViewModel GetUserKudosInformationById(string id = null)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }

            var userKudosDto = _kudosService.GetUserKudosInformationById(id, GetUserAndOrganization().OrganizationId);
            var userKudosViewModel = _mapper.Map<UserKudosDTO, UserKudosViewModel>(userKudosDto);

            var monthlyStatistics = _kudosService.GetMonthlyKudosStatistics(id);
            userKudosViewModel.SentKudos = monthlyStatistics[0];
            userKudosViewModel.AvailableKudos = monthlyStatistics[1];

            return userKudosViewModel;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        public IHttpActionResult GetApprovedKudosList(string id = null)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }

            try
            {
                var userKudosInformationDto = _kudosService.GetApprovedKudosList(id, GetUserAndOrganization().OrganizationId);
                var result = _mapper.Map<IEnumerable<UserKudosInformationDTO>, IEnumerable<UserKudosInformationViewModel>>(userKudosInformationDto);
                return Ok(result);
            }
            catch (ValidationException e) { return BadRequestWithError(e); }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneHour)]
        public IEnumerable<WallKudosLogViewModel> GetLastKudosLogRecords()
        {
            var userAndOrg = GetUserAndOrganization();
            var wallKudosLogsDto = _kudosService.GetLastKudosLogsForWall(userAndOrg);
            return _mapper.Map<IEnumerable<WallKudosLogDTO>, IEnumerable<WallKudosLogViewModel>>(wallKudosLogsDto);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneHour)]
        public IEnumerable<KudosBasicDataViewModel> GetKudosStats(int months, int amount)
        {
            if (months <= 0 || amount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var kudosStatsDto = _kudosService.GetKudosStats(months, amount, GetUserAndOrganization().OrganizationId);
            var result = _mapper.Map<IEnumerable<KudosBasicDataDTO>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneHour)]
        public IEnumerable<KudosListBasicDataViewModel> GetKudosWidgetStats(int tabOneMonths, int tabOneAmount, int tabTwoMonths, int tabTwoAmount)
        {
            var result = new List<KudosListBasicDataViewModel>();
            if (tabOneMonths <= 0 || tabOneAmount <= 0 || tabTwoMonths <= 0 || tabTwoAmount <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            result.Add(CalculateStats(tabOneMonths, tabOneAmount));
            result.Add(CalculateStats(tabTwoMonths, tabTwoAmount));
            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public IHttpActionResult GetKudosLogAsExcel([FromUri] KudosLogsFilterViewModel filter)
        {
            var filterDto = _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDTO>(filter);
            SetOrganizationAndUser(filterDto);
            try
            {
                var stream = new ByteArrayContent(_kudosExportService.ExportToExcel(filterDto));
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
            var welcomeKudosDTO = _kudosService.GetWelcomeKudos();

            var result = _mapper.Map<KudosWelcomeDTO, WelcomeKudosViewModel>(welcomeKudosDTO);

            return Ok(result);
        }

        private KudosListBasicDataViewModel CalculateStats(int months, int amount)
        {
            var kudosStatsDto = _kudosService.GetKudosStats(months, amount, GetUserAndOrganization().OrganizationId);
            var stats = _mapper.Map<IEnumerable<KudosBasicDataDTO>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return new KudosListBasicDataViewModel()
            {
                Users = stats,
                Months = months
            };
        }
    }
}