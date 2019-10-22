using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Args;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Lotteries;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

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
        [Route("Paged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        public PagedViewModel<LotteryDetailsDTO> GetPagedLotteries(string filter = "", int page = 1, int pageSize = ConstWebApi.DefaultPageSize)
        {
            var args = new GetPagedLotteriesArgs { Filter = filter, PageNumber = page, PageSize = pageSize, UserOrg = GetUserAndOrganization() };
            var pagedLotteries = _lotteryService.GetPagedLotteries(args);

            return new PagedViewModel<LotteryDetailsDTO>
            {
                PagedList = pagedLotteries,
                PageCount = pagedLotteries.PageCount,
                ItemCount = pagedLotteries.TotalItemCount,
                PageSize = pageSize
            };
        }

        [HttpGet]
        [Route("Details")]
        public IHttpActionResult GetLottery(int id)
        {
            try
            {
                var lotteryDTO = _lotteryService.GetLotteryDetails(id, GetUserAndOrganization());

                var lotteryViewModel = _mapper.Map<LotteryDetailsDTO, LotteryDetailsViewModel>(lotteryDTO);

                return Ok(lotteryViewModel);
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
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
                await _lotteryService.CreateLottery(createLotteryDTO);
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("Abort")]
        public IHttpActionResult Abort(int id)
        {
            _lotteryService.AbortLottery(id, GetUserAndOrganization());

            return Ok();
        }

        [HttpPatch]
        [Route("{id}/Refund")]
        public IHttpActionResult RefundParticipants(int id)
        {
            _lotteryService.RefundParticipants(id, GetUserAndOrganization());

            return Ok();
        }

        [HttpGet]
        [Route("{id}/Status")]
        public IHttpActionResult GetStatus(int id)
        {
            var status =_lotteryService.GetLotteryStatus(id);

            return Ok(new { status });
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
            catch (LotteryException e)
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
