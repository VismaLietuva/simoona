using System;
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Birthday;
using Shrooms.WebViewModels.Models.Birthday;
using WebApi.OutputCache.V2;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    public class BirthdaysController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IBirthdayService _birthdayService;

        public BirthdaysController(IMapper mapper, IBirthdayService birthdayService)
        {
            _mapper = mapper;
            _birthdayService = birthdayService;
        }

        [HttpGet]
        [CacheOutput(ServerTimeSpan = ConstWebApi.OneHour)]
        [PermissionAuthorize(Permission = BasicPermissions.Birthday)]
        public IEnumerable<BirthdayViewModel> GetWeeklyBirthdays()
        {
            var todayDate = DateTime.UtcNow;
            var birthdaysDTO = _birthdayService.GetWeeklyBirthdays(todayDate);
            var birthdays = _mapper.Map<IEnumerable<BirthdayDTO>, IEnumerable<BirthdayViewModel>>(birthdaysDTO);
            return birthdays;
        }
    }
}