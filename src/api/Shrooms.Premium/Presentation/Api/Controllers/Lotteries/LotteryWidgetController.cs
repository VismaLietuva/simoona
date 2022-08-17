using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.Premium.Presentation.Api.Controllers.Lotteries
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
        [PermissionAwareCacheOutputFilter(BasicPermissions.Lottery, ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IEnumerable<LotteryWidgetViewModel>> Get()
        {
            var lotteriesDto = await _lotteryService.GetRunningLotteriesAsync(GetUserAndOrganization());

            return _mapper.Map<IEnumerable<LotteryDetailsDto>, IEnumerable<LotteryWidgetViewModel>>(lotteriesDto);
        }
    }
}
