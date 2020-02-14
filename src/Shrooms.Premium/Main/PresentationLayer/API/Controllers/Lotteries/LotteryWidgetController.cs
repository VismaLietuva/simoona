using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Lotteries;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Main.PresentationLayer.API.Controllers.Lotteries
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
        public IEnumerable<LotteryWidgetViewModel> Get()
        {
            var lotteriesDTO = _lotteryService.GetRunningLotteries(GetUserAndOrganization());

            return _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryWidgetViewModel>>(lotteriesDTO);
        }
    }
}
