using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Birthdays;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Presentation.WebViewModels.Models.Birthday;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;
using Shrooms.Presentation.WebViewModels.Models.Wall.Widgets;

namespace Shrooms.Presentation.Api.Controllers.Wall
{
    [Authorize]
    [RoutePrefix("WallWidgets")]
    public class WallWidgetsController : BaseController
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
        [PermissionAwareCacheOutputFilter(BasicPermissions.Kudos, BasicPermissions.Birthday, BasicPermissions.KudosBasket, ServerTimeSpan = WebApiConstants.OneHour)]
        public WidgetsViewModel Get([FromUri]GetWidgetsViewModel getWidgetsViewModel)
        {
            var userAndOrganization = GetUserAndOrganization();
            return new WidgetsViewModel
            {
                KudosWidgetStats = DefaultIfNotAuthorized(userAndOrganization, BasicPermissions.Kudos,
                    () => GetKudosWidgetStats(
                        getWidgetsViewModel.KudosTabOneMonths,
                        getWidgetsViewModel.KudosTabOneAmount,
                        getWidgetsViewModel.KudosTabTwoMonths,
                        getWidgetsViewModel.KudosTabTwoAmount)),
                LastKudosLogRecords = DefaultIfNotAuthorized(userAndOrganization, BasicPermissions.Kudos, GetLastKudosLogRecords),
                WeeklyBirthdays = DefaultIfNotAuthorized(userAndOrganization, BasicPermissions.Birthday, GetWeeklyBirthdays),
                KudosBasketWidget = DefaultIfNotAuthorized(userAndOrganization, BasicPermissions.KudosBasket, GetKudosBasketWidget)
            };
        }

        private T DefaultIfNotAuthorized<T>(UserAndOrganizationDTO userAndOrganization, string permission, Func<T> valueFactory)
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
            var result = new List<KudosListBasicDataViewModel>
            {
                CalculateStats(tabOneMonths, tabOneAmount),
                CalculateStats(tabTwoMonths, tabTwoAmount)
            };
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