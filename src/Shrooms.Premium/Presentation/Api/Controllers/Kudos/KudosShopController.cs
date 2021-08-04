using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Domain.Services.KudosShop;
using Shrooms.Premium.Presentation.WebViewModels.KudosShop;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.Kudos
{
    [Authorize]
    [RoutePrefix("KudosShop")]
    public class KudosShopController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IKudosShopService _kudosShopService;

        public KudosShopController(
            IMapper mapper,
            IKudosShopService kudosShopService)
        {
            _mapper = mapper;
            _kudosShopService = kudosShopService;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosShop)]
        public async Task<IHttpActionResult> CreateItem(KudosShopItemViewModel kudosShopItemViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createKudosShopItem = _mapper.Map<KudosShopItemViewModel, KudosShopItemDto>(kudosShopItemViewModel);
            SetOrganizationAndUser(createKudosShopItem);

            try
            {
                await _kudosShopService.CreateItemAsync(createKudosShopItem);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Get")]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosShop)]
        public async Task<IHttpActionResult> GetItem(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid request");
            }

            try
            {
                var itemDto = await _kudosShopService.GetItemAsync(id, GetUserAndOrganization());
                var result = _mapper.Map<KudosShopItemDto, KudosShopItemViewModel>(itemDto);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("All")]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosShop)]
        public async Task<IHttpActionResult> GetAllItems()
        {
            var userOrganization = GetUserAndOrganization();
            var allListDto = await _kudosShopService.GetAllItemsAsync(userOrganization);
            var result = _mapper.Map<IEnumerable<KudosShopItemDto>, IEnumerable<KudosShopItemViewModel>>(allListDto);
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosShop)]
        public async Task<IHttpActionResult> UpdateItem(KudosShopItemViewModel kudosShopItemViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var kudosShopItem = _mapper.Map<KudosShopItemViewModel, KudosShopItemDto>(kudosShopItemViewModel);
            SetOrganizationAndUser(kudosShopItem);

            try
            {
                await _kudosShopService.UpdateItemAsync(kudosShopItem);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.KudosShop)]
        public async Task<IHttpActionResult> DeleteItem(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid request");
            }

            try
            {
                await _kudosShopService.DeleteItemAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}