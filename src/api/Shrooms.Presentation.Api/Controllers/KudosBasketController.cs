using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Domain.Exceptions.Exceptions.KudosBaskets;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Controllers.Kudos;
using Shrooms.Presentation.Common.Controllers.Wall;
using Shrooms.Presentation.Common.Filters;
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
            var result = _mapper.Map<IList<KudosBasketLogDto>, IList<KudosBasketLogViewModel>>(kudosDonations);
            return Ok(result);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public async Task<IHttpActionResult> CreateNewKudosBasket(KudosBasketCreateViewModel newBasket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newBasketDto = _mapper.Map<KudosBasketCreateViewModel, KudosBasketCreateDto>(newBasket);
            SetOrganizationAndUser(newBasketDto);
            newBasketDto = await _kudosBasketService.CreateNewBasketAsync(newBasketDto);
            newBasket = _mapper.Map<KudosBasketCreateDto, KudosBasketCreateViewModel>(newBasketDto);
            return Ok(newBasket);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.KudosBasket)]
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneHour)]
        public async Task<IHttpActionResult> GetKudosBasketWidget()
        {
            var basket = await _kudosBasketService.GetKudosBasketWidgetAsync(GetUserAndOrganization());
            var basketViewModel = basket == null ? null : _mapper.Map<KudosBasketDto, KudosBasketWidgetViewModel>(basket);
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
                var basketViewModel = _mapper.Map<KudosBasketDto, KudosBasketViewModel>(basket);
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
        public async Task<IHttpActionResult> DeleteKudosBasket()
        {
            var userAndOrganization = GetUserAndOrganization();
            await _kudosBasketService.DeleteKudosBasketAsync(userAndOrganization);
            return Ok();
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        public async Task<IHttpActionResult> EditKudosBasket(KudosBasketEditViewModel editedBasket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var editedBasketDto = _mapper.Map<KudosBasketEditViewModel, KudosBasketEditDto>(editedBasket);
            SetOrganizationAndUser(editedBasketDto);
            await _kudosBasketService.EditKudosBasketAsync(editedBasketDto);
            return Ok();
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.KudosBasket)]
        [InvalidateCacheOutput("Get", typeof(WallWidgetsController))]
        [InvalidateCacheOutput("GetLastKudosLogRecords", typeof(KudosController))]
        public async Task<IHttpActionResult> MakeDonation(KudosBasketDonateViewModel donation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var donationDto = _mapper.Map<KudosBasketDonateViewModel, KudosBasketDonationDto>(donation);
                SetOrganizationAndUser(donationDto);
                await _kudosBasketService.MakeDonationAsync(donationDto);
            }
            catch (KudosBasketException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
