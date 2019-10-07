using AutoMapper;
using PagedList;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.API.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
    public class LotteryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILotteryService _lotteryService;

        public LotteryController(IMapper mapper, ILotteryService lotteryService)
        {
            _mapper = mapper;
            _lotteryService = lotteryService;
        }

        [Route("All")]
        public IHttpActionResult GetAllLotteries()
        {
            var lotteriesDTO = _lotteryService.GetLotteries(GetUserAndOrganization());

            var result = _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryDetailsViewModel>>(lotteriesDTO);

            return Ok(result);
        }

        [HttpGet]
        [Route("GetPaged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        public PagedViewModel<LotteryDetailsViewModel> GetPaged(string filter = "", int page = 1, int pageSize = ConstWebApi.DefaultPageSize)
        {
            var lotteriesDTO = _lotteryService.GetFilteredLotteries(GetUserAndOrganization(), filter);

            var result = _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryDetailsViewModel>>(lotteriesDTO);

            var pagedLotteries = result.ToPagedList(page, pageSize);

            var pagedModel = new PagedViewModel<LotteryDetailsViewModel>
            {
                PagedList = pagedLotteries,
                PageCount = pagedLotteries.PageCount,
                ItemCount = pagedLotteries.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IHttpActionResult> CreateLottery(CreateLotteryViewModel lotteryViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createLotteryDTO = _mapper.Map<CreateLotteryViewModel, CreateLotteryDTO>(lotteryViewModel);
            SetOrganizationAndUser(createLotteryDTO);
            try
            {
                var createLottery = await _lotteryService.CreateLottery(createLotteryDTO);
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                _lotteryService.RemoveLottery(id, GetUserAndOrganization());
                return Ok();
            }
            catch(LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("UpdateDrafted")]
        public IHttpActionResult UpdateDrafted(EditDraftedLotteryViewModel editLotteryViewModel)
        {
            try
            {
                var editDraftedLotteryDTO = _mapper.Map<EditDraftedLotteryViewModel, EditDraftedLotteryDTO>(editLotteryViewModel);
                SetOrganizationAndUser(editDraftedLotteryDTO);
                _lotteryService.EditDraftedLottery(editDraftedLotteryDTO);

                return Ok();
            }
            catch(LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("UpdateStarted")]
        public IHttpActionResult UpdateStarted(EditStartedLotteryViewModel editLotteryViewModel)
        {
            try
            {
                var editStartedLotteryDTO = _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(editLotteryViewModel);
                SetOrganizationAndUser(editStartedLotteryDTO);
                _lotteryService.EditStartedLottery(editStartedLotteryDTO);
                return Ok();
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
