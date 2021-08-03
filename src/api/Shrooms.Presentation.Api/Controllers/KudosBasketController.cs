using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Domain.Exceptions.Exceptions.KudosBaskets;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Presentation.Api.Controllers.Kudos;
using Shrooms.Presentation.Api.Controllers.Wall;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.KudosBaskets;
using Shrooms.Presentation.WebViewModels.Models.Wall.Widgets;
using WebApi.OutputCache.V2;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [AutoInvalidateCacheOutput]
    public class KudosBasketController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IKudosBasketService _kudosBasketService;

        public KudosBasketController(IMapper mapper, IKudosBasketService kudosBasketService)
        {
            _mapper = mapper;
            _kudosBasketService = kudosBasketService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.KudosBasket)]
        public async Task<IHttpActionResult> GetDonations()
        {
            var userAndOrg = GetUserAndOrganization();

            var kudosDonations = await _kudosBasketService.GetDonationsAsync(userAndOrg);
            var result = _mapper.Map<IList<KudosBasketLogDTO>, IList<KudosBasketLogViewModel>>(kudosDonations);
            return Ok(result);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult CreateNewKudosBasket(KudosBasketCreateViewModel newBasket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newBasketDto = _mapper.Map<KudosBasketCreateViewModel, KudosBasketCreateDTO>(newBasket);
            SetOrganizationAndUser(newBasketDto);
            newBasketDto = _kudosBasketService.CreateNewBasket(newBasketDto);
            newBasket = _mapper.Map<KudosBasketCreateDTO, KudosBasketCreateViewModel>(newBasketDto);
            return Ok(newBasket);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.KudosBasket)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IHttpActionResult> GetKudosBasketWidget()
        {
            var basket = await _kudosBasketService.GetKudosBasketWidgetAsync(GetUserAndOrganization());
            var basketViewModel = basket == null ? null : _mapper.Map<KudosBasketDTO, KudosBasketWidgetViewModel>(basket);
            return Ok(basketViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        public async Task<IHttpActionResult> GetKudosBasket()
        {
            var userAndOrg = GetUserAndOrganization();
            try
            {
                var basket = await _kudosBasketService.GetKudosBasketAsync(userAndOrg);
                var basketViewModel = _mapper.Map<KudosBasketDTO, KudosBasketViewModel>(basket);
                return Ok(basketViewModel);
            }
            catch (KudosBasketException e)
            {
                return Ok(e);
            }
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult DeleteKudosBasket()
        {
            var userAndOrganization = GetUserAndOrganization();
            _kudosBasketService.DeleteKudosBasketAsync(userAndOrganization);
            return Ok();
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public IHttpActionResult EditKudosBasket(KudosBasketEditViewModel editedBasket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var editedBasketDto = _mapper.Map<KudosBasketEditViewModel, KudosBasketEditDTO>(editedBasket);
            SetOrganizationAndUser(editedBasketDto);
            _kudosBasketService.EditKudosBasketAsync(editedBasketDto);
            return Ok();
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        [InvalidateCacheOutput("GetLastKudosLogRecords", typeof(KudosController))]
        public IHttpActionResult MakeDonation(KudosBasketDonateViewModel donation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var donationDto = _mapper.Map<KudosBasketDonateViewModel, KudosBasketDonationDTO>(donation);
                SetOrganizationAndUser(donationDto);
                _kudosBasketService.MakeDonationAsync(donationDto);
            }
            catch (KudosBasketException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
