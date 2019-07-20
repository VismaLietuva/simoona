using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Helpers;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.DataTransferObjects.Models.KudosBasket;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Domain.Services.Permissions;
using Shrooms.WebViewModels.Models.Birthday;
using Shrooms.WebViewModels.Models.Kudos;
using Shrooms.WebViewModels.Models.Users.Kudos;
using Shrooms.WebViewModels.Models.Wall.Widgets;

namespace Shrooms.API.Controllers.Kudos
{
    [Authorize]
    [RoutePrefix("WallWidgets")]
    public class WallWidgetsController: BaseController
    {
        private readonly IMapper _mapper;
        private readonly IKudosService _kudosService;
        private readonly IPermissionService _permissionService;
        private readonly IKudosBasketService _kudosBasketService;
        private readonly IBirthdayService _birthdayService;

        public WallWidgetsController(
            IMapper mapper, 
            IKudosService kudosService, 
            IPermissionService permissionService, 
            IKudosBasketService kudosBasketService,
            IBirthdayService birthdayService)
        {
            _mapper = mapper;
            _kudosService = kudosService;
            _permissionService = permissionService;
            _kudosBasketService = kudosBasketService;
            _birthdayService = birthdayService;
        }

        [HttpGet]
        [PermissionAwareCacheOutputFilter(BasicPermissions.Kudos, BasicPermissions.Birthday, BasicPermissions.KudosBasket, ServerTimeSpan = ConstWebApi.OneHour)]
        public WidgetsViewModel Get([FromUri]GetWidgetsViewModel getWidgetsViewModel)
        {
            var userAndOrganization = GetUserAndOrganization();
            return new WidgetsViewModel
            {
                KudosWidgetStats = DefaultIfNotAuthroized(userAndOrganization, BasicPermissions.Kudos, 
                    () => GetKudosWidgetStats(
                        getWidgetsViewModel.KudosTabOneMonths, 
                        getWidgetsViewModel.KudosTabOneAmount, 
                        getWidgetsViewModel.KudosTabTwoMonths, 
                        getWidgetsViewModel.KudosTabTwoAmount)),
                LastKudosLogRecords = DefaultIfNotAuthroized(userAndOrganization, BasicPermissions.Kudos, GetLastKudosLogRecords),
                WeeklyBirthdays = DefaultIfNotAuthroized(userAndOrganization, BasicPermissions.Birthday, GetWeeklyBirthdays),
                KudosBasketWidget = DefaultIfNotAuthroized(userAndOrganization, BasicPermissions.KudosBasket, GetKudosBasketWidget)
            };
        }

        private T DefaultIfNotAuthroized<T>(UserAndOrganizationDTO userAndOrganization, string permission, Func<T> valueFactory)
        {
            return _permissionService.UserHasPermission(userAndOrganization, permission) ? valueFactory() : default;
        }

        private IEnumerable<WallKudosLogViewModel> GetLastKudosLogRecords()
        {
            var userAndOrg = GetUserAndOrganization();
            var wallKudosLogsDto = _kudosService.GetLastKudosLogsForWall(userAndOrg);
            return _mapper.Map<IEnumerable<WallKudosLogDTO>, IEnumerable<WallKudosLogViewModel>>(wallKudosLogsDto);
        }

        public KudosBasketWidgetViewModel GetKudosBasketWidget()
        {
            var basket = _kudosBasketService.GetKudosBasketWidget(GetUserAndOrganization());
            return basket == null ? null : _mapper.Map<KudosBasketDTO, KudosBasketWidgetViewModel>(basket);
        }

        private IEnumerable<BirthdayViewModel> GetWeeklyBirthdays()
        {
            var todayDate = DateTime.UtcNow;
            var birthdaysDTO = _birthdayService.GetWeeklyBirthdays(todayDate);
            var birthdays = _mapper.Map<IEnumerable<BirthdayDTO>, IEnumerable<BirthdayViewModel>>(birthdaysDTO);
            return birthdays;
        }

        private IEnumerable<KudosListBasicDataViewModel> GetKudosWidgetStats(int tabOneMonths, int tabOneAmount, int tabTwoMonths, int tabTwoAmount)
        {
            var result = new List<KudosListBasicDataViewModel>();
            result.Add(CalculateStats(tabOneMonths, tabOneAmount));
            result.Add(CalculateStats(tabTwoMonths, tabTwoAmount));
            return result;
        }

        private KudosListBasicDataViewModel CalculateStats(int months, int amount)
        {
            var kudosStatsDto = _kudosService.GetKudosStats(months, amount, User.Identity.GetOrganizationId());
            var stats = _mapper.Map<IEnumerable<KudosBasicDataDTO>, IEnumerable<KudosBasicDataViewModel>>(kudosStatsDto);
            return new KudosListBasicDataViewModel()
            {
                Users = stats,
                Months = months
            };
        }
    }
}