﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;
using WebApi.OutputCache.V2;

namespace Shrooms.Premium.Presentation.Api.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
    public class LotteryController : BaseController
    {
        private readonly IMapper _mapper;

        private readonly ILotteryService _lotteryService;
        private readonly ILotteryExportService _lotteryExportService;

        public LotteryController(IMapper mapper, ILotteryService lotteryService, ILotteryExportService lotteryExportService)
        {
            _mapper = mapper;
            _lotteryService = lotteryService;
            _lotteryExportService = lotteryExportService;
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
        public IHttpActionResult GetPagedLotteries(string filter = "", int page = 1, int pageSize = WebApiConstants.DefaultPageSize)
        {
            var args = new GetPagedLotteriesArgs { Filter = filter, PageNumber = page, PageSize = pageSize, UserOrg = GetUserAndOrganization() };
            var pagedLotteriesDTO = _lotteryService.GetPagedLotteries(args);

            var pagedLotteriesViewModel = new PagedViewModel<LotteryDetailsDTO>
            {
                PagedList = pagedLotteriesDTO,
                PageCount = pagedLotteriesDTO.PageCount,
                ItemCount = pagedLotteriesDTO.TotalItemCount,
                PageSize = pageSize
            };

            return Ok(pagedLotteriesViewModel);
        }

        [HttpGet]
        [Route("{id}/Details")]
        public IHttpActionResult GetLottery(int id)
        {
            var lotteryDTO = _lotteryService.GetLotteryDetails(id, GetUserAndOrganization());

            if (lotteryDTO == null)
            {
                return Content((HttpStatusCode)422, "Lottery with such ID was not found");
            }

            var lotteryViewModel = _mapper.Map<LotteryDetailsDTO, LotteryDetailsViewModel>(lotteryDTO);

            return Ok(lotteryViewModel);
        }

        [HttpPost]
        [Route("Create")]
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
        public async Task<IHttpActionResult> CreateLottery(CreateLotteryViewModel lotteryViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createLotteryDTO = _mapper.Map<CreateLotteryViewModel, LotteryDTO>(lotteryViewModel);
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

        [HttpPatch]
        [Route("{id}/Abort")]
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
        public IHttpActionResult Abort(int id)
        {
            var success = _lotteryService.AbortLottery(id, GetUserAndOrganization());
            if (!success)
            {
                return Content((HttpStatusCode)422, "Lottery with such ID was not found");
            }

            return Ok();
        }

        [HttpPost]
        [Route("Enter")]
        public async Task<IHttpActionResult> BuyLotteryTicket(BuyLotteryTicketViewModel lotteryTickets)
        {
            try
            {
                var buyLotterTicketDTO = _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(lotteryTickets);

                await _lotteryService.BuyLotteryTicketAsync(buyLotterTicketDTO, GetUserAndOrganization());

                return Ok();
            }
            catch (LotteryException ex)
            {
                return BadRequest(ex.Message);
            }
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
            var status = _lotteryService.GetLotteryStatus(id, GetUserAndOrganization());

            return Ok(status);
        }

        [HttpPut]
        [Route("UpdateDrafted")]
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
        public IHttpActionResult UpdateDrafted(EditDraftedLotteryViewModel editLotteryViewModel)
        {
            try
            {
                var editDraftedLotteryDTO = _mapper.Map<EditDraftedLotteryViewModel, LotteryDTO>(editLotteryViewModel);
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
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
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

        [HttpPatch]
        [Route("{id}/Finish")]
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
        public async Task<IHttpActionResult> FinishLottery(int id)
        {
            try
            {
                await _lotteryService.FinishLotteryAsync(id, GetUserAndOrganization());

                return Ok();
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("{id}/Stats")]
        public IHttpActionResult LotteryStats(int id)
        {
            var lotteryStats = _lotteryService.GetLotteryStats(id, GetUserAndOrganization());

            if (lotteryStats == null)
            {
                return Content((HttpStatusCode)422, "Lottery with such ID was not found");
            }

            return Ok(lotteryStats);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        [Route("Export")]
        public IHttpActionResult Export(int lotteryId)
        {
            try
            {
                var stream = new ByteArrayContent(_lotteryExportService.ExportParticipants(lotteryId, GetUserAndOrganization()));
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = stream };
                return ResponseMessage(result);
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}