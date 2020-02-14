using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Domain.Services.KudosShop;
using Shrooms.Premium.Presentation.WebViewModels.Models.KudosShop;
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

            var createKudosShopItemDTO = _mapper.Map<KudosShopItemViewModel, KudosShopItemDTO>(kudosShopItemViewModel);
            SetOrganizationAndUser(createKudosShopItemDTO);

            try
            {
                await _kudosShopService.CreateItem(createKudosShopItemDTO);
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
                var itemDto = await _kudosShopService.GetItem(id, GetUserAndOrganization());
                var result = _mapper.Map<KudosShopItemDTO, KudosShopItemViewModel>(itemDto);
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
            var allListDto = await _kudosShopService.GetAllItems(userOrganization);
            var result = _mapper.Map<IEnumerable<KudosShopItemDTO>, IEnumerable<KudosShopItemViewModel>>(allListDto);
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

            var itemDTO = _mapper.Map<KudosShopItemViewModel, KudosShopItemDTO>(kudosShopItemViewModel);
            SetOrganizationAndUser(itemDTO);

            try
            {
                await _kudosShopService.UpdateItem(itemDTO);
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
                await _kudosShopService.DeleteItem(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}