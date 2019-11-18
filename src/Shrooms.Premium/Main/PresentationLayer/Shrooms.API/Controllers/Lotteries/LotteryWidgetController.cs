using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.API.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("LotteryWidget")]
    public class LotteryWidgetController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILotteryService _lotteryService;
        public LotteryWidgetController(IMapper mapper, ILotteryService lotteryService)
        {
            _mapper = mapper;
            _lotteryService = lotteryService;
        }
        
        [HttpGet]
        [PermissionAwareCacheOutputFilter(BasicPermissions.Lottery, ServerTimeSpan = ConstWebApi.OneHour)]
        public IEnumerable<LotteryWidgetViewModel> Get()
        {
            var lotteriesDTO = _lotteryService.GetRunningLotteries(GetUserAndOrganization());

            return _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryWidgetViewModel>>(lotteriesDTO);
        }
    }
}
