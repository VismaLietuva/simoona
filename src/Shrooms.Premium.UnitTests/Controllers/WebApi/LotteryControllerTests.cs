using NSubstitute;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.API.Controllers.Lotteries;
using AutoMapper;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using System.Security.Claims;
using System.Net;
using Shrooms.DataTransferObjects.Models;
using Shrooms.WebViewModels.Models.Lotteries;
using System.Web.Http.Results;
using Shrooms.Domain.Services.Args;
using Shrooms.Constants.WebApi;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute.ExceptionExtensions;
using PagedList;
using Shrooms.WebViewModels.Models;

namespace Shrooms.Premium.UnitTests.Controllers.WebApi
{
    public class LotteryControllerTests
    {
        private LotteryController _lotteryController;

        private IMapper _mapper;
        private ILotteryService _lotteryService;

        [SetUp]
        public void TestInitializers()
        {
            _mapper = Substitute.For<IMapper>();
            _lotteryService = Substitute.For<ILotteryService>();

            _lotteryController = new LotteryController(_mapper, _lotteryService);

            _lotteryController.ControllerContext = Substitute.For<HttpControllerContext>();
            _lotteryController.Request = new HttpRequestMessage();
            _lotteryController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _lotteryController.Request.SetConfiguration(new HttpConfiguration());
            _lotteryController.RequestContext.Principal = 
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));

        }

        [TestCase]
        public void GetAllLotteries_Should_Return_Ok_With_IEnumerable_Of_LotteryDetails_ViewModel()
        {
            //Arrange
            _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryDetailsViewModel>>(LotteryDetailsDTO)
           .Returns(LotteryDetailsViewModel);

            _lotteryService.GetLotteries(GetUserAndOrganization())
                .Returns(LotteryDetailsDTO);

            //Act
            var response = _lotteryController.GetAllLotteries();

            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<LotteryDetailsViewModel>>>(response);
            _lotteryService.Received(1).GetLotteries(GetUserAndOrganization());
        }

        [TestCase]
        public void GetLottery_Should_Return_Ok()
        {
            // 
            var lotteryViewModel = new LotteryDetailsViewModel
            {
                Id = 2,
                Status = 1,
                Title = "Hello",
            };
            var lotteryDTO = new LotteryDetailsDTO
            {
                Id = 2,
                Status = 1,
                Title = "Hello",
            };

            _mapper.Map<LotteryDetailsDTO, LotteryDetailsViewModel>(lotteryDTO)
                  .Returns(lotteryViewModel);

            _lotteryService.GetLotteryDetails(2).Returns(lotteryDTO);

            //
            var response = _lotteryController.GetLottery(2);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryDetailsViewModel>>(response);
            _lotteryService.Received(1).GetLotteryDetails(2);
        }

        [TestCase]
        public void GetLottery_Should_Return_Unprocessable_Entity_Error()
        {
            //
            _lotteryService.GetLotteryDetails(3000).Returns(a => null);

            //
            var response = _lotteryController.GetLottery(3000);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            _lotteryService.Received(1).GetLotteryDetails(3000);

        }

        [TestCase]
        public void Abort_Should_Return_Ok()
        {
            //            
            _lotteryService.AbortLottery(2, GetUserAndOrganization()).Returns(true);

            //
            var response = _lotteryController.Abort(2);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).AbortLottery(2, GetUserAndOrganization());
        }

        [TestCase]
        public void Abort_Should_Return_Unprocessable_Entity_Error()
        {
            //
            _lotteryService.AbortLottery(5, GetUserAndOrganization()).Returns(false);

            //
            var response = _lotteryController.Abort(5);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            _lotteryService.Received(1).AbortLottery(5, GetUserAndOrganization());
        }

        [TestCase]
        public async Task CreateLottery_Should_Return_Invalid_Model_State()
        {
            //
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new CreateLotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, CreateLotteryDTO>(lotteryViewModel)
               .Returns(lotteryDTO);
            //
            _lotteryController.ModelState.AddModelError("model", "error");

            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            //
            Assert.IsInstanceOf<InvalidModelStateResult>(response);
            await _lotteryService.DidNotReceive().CreateLottery(lotteryDTO);
        }

        [TestCase]
        public async Task CreateLottery_Should_Return_Ok()
        {
            //
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new CreateLotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, CreateLotteryDTO>(lotteryViewModel)
               .Returns(lotteryDTO);
            //
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).CreateLottery(lotteryDTO);
        }

        [TestCase]
        public async Task CreateLottery_Should_Return_Bad_Request()
        {
            //
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new CreateLotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, CreateLotteryDTO>(lotteryViewModel)
               .Returns(lotteryDTO);
            _lotteryService.CreateLottery(lotteryDTO).Throws(new LotteryException("Exception"));
            //
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            //
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).CreateLottery(lotteryDTO);
        }

        [TestCase]
        public async Task BuyLotteryTicket_Should_Return_Ok()
        {
            //
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };
            var ticketDTO = new BuyLotteryTicketDTO
            {
                LotteryId = 1,
                Tickets = 5,
            };
            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(ticketViewModel)
                .Returns(ticketDTO);

            //
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDTO, GetUserAndOrganization());

        }

        [TestCase]
        public async Task BuyLotteryTicket_Should_Return_Bad_Request()
        {
            //
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };
            var ticketDTOModel = new BuyLotteryTicketDTO
            {
                LotteryId = 1,
                Tickets = 5,
            };
            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(ticketViewModel)
                .Returns(ticketDTOModel);

            _lotteryService.BuyLotteryTicketAsync(ticketDTOModel, GetUserAndOrganization()).Throws(new LotteryException("Exception"));
          
            //
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDTOModel, GetUserAndOrganization());

        }

        [TestCase]
        public void GetPagedLotteries_Should_Return_Ok()
        {
            //
            var args = new GetPagedLotteriesArgs
            {
                Filter = "",
                PageNumber = 1,
                PageSize = 10,
                UserOrg = GetUserAndOrganization()
            };
            _lotteryService.GetPagedLotteries(args).Returns(LotteryDetailsDTO.ToPagedList(args.PageNumber, args.PageSize));

            //
            var response = _lotteryController.GetPagedLotteries("", 1, 10);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedViewModel<LotteryDetailsDTO>>>(response);
            _lotteryService.Received(1).GetPagedLotteries(Arg.Any<GetPagedLotteriesArgs>());

        }

        [TestCase]
        public void RefundParticipants_Should_Return_Ok()
        {
            //

            //
            var response = _lotteryController.RefundParticipants(1337);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).RefundParticipants(1337, GetUserAndOrganization());
        }

        [TestCase]
        public async Task FinishLottery_Should_Return_Ok()
        {
            //

            //
            var response = await _lotteryController.FinishLottery(37);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37);
        }

        [TestCase]
        public async Task FinishLottery_Should_Return_Bad_Request()
        {
            //
            _lotteryService.FinishLotteryAsync(37).Throws(new LotteryException("Exception"));
            //
            var response = await _lotteryController.FinishLottery(37);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37);
        }

        [TestCase]
        public void UpdateDrafted_Should_Return_Ok()
        {
            //
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new EditDraftedLotteryDTO
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, EditDraftedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            //
            var response = _lotteryController.UpdateDrafted(lotteryViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).EditDraftedLottery(lotteryDto);

        }

        [TestCase]
        public void UpdateDrafted_Should_Return_Bad_Request()
        {
            //
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new EditDraftedLotteryDTO
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, EditDraftedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            _lotteryService.EditDraftedLottery(lotteryDto).Throws(new LotteryException("Exception"));

            //
            var response = _lotteryController.UpdateDrafted(lotteryViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            _lotteryService.Received(1).EditDraftedLottery(lotteryDto);

        }

        [TestCase]
        public void UpdateStarted_Should_Return_Ok()
        {
            //
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31,
            };

            var lotteryDto = new EditStartedLotteryDTO
            {
                Id = 31,
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            //
            var response = _lotteryController.UpdateStarted(lotteryViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).EditStartedLottery(lotteryDto);

        }

        [TestCase]
        public void UpdateStarted_Should_Return_Bad_Request()
        {
            //
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31,
            };

            var lotteryDto = new EditStartedLotteryDTO
            {
                Id = 31,
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);
            _lotteryService.EditStartedLottery(lotteryDto).Throws(new LotteryException("Exception"));
            //
            var response = _lotteryController.UpdateStarted(lotteryViewModel);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            _lotteryService.Received(1).EditStartedLottery(lotteryDto);

        }
        
        [TestCase]
        public void LotteryStats_Should_Return_Ok()
        {
            //
            var lotteryStats = new LotteryStatsDTO()
            {
                KudosSpent = 60,
                TicketsSold = 30,
                TotalParticipants = 15
            };
            _lotteryService.GetLotteryStats(13).Returns(lotteryStats);

            //
            var response = _lotteryController.LotteryStats(13);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryStatsDTO>>(response);
            _lotteryService.Received(1).GetLotteryStats(13);

        }

        [TestCase]
        public void LotteryStats_Should_Return_Unprocessable_Entity_Error()
        {
            //
            var lotteryStats = new LotteryStatsDTO()
            {
                KudosSpent = 60,
                TicketsSold = 30,
                TotalParticipants = 15
            };
            _lotteryService.GetLotteryStats(13).Returns(x => null);

            //
            var response = _lotteryController.LotteryStats(13);

            //
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            _lotteryService.Received(1).GetLotteryStats(13);

        }

        private IEnumerable<LotteryDetailsDTO> LotteryDetailsDTO => new List<LotteryDetailsDTO>
            {
                new LotteryDetailsDTO() { Id = 1,  Status = 2, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor",  EntryFee = -5 },
                new LotteryDetailsDTO() { Id = 2,  Status = 2, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", EntryFee = 2 },
                new LotteryDetailsDTO() { Id = 3,  Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
                new LotteryDetailsDTO() { Id = 4,  Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
                new LotteryDetailsDTO() { Id = 5,  Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
                new LotteryDetailsDTO() { Id = 6,  Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
                new LotteryDetailsDTO() { Id = 7,  Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
                new LotteryDetailsDTO() { Id = 8,  Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 },
            };
        private IEnumerable<LotteryDetailsViewModel> LotteryDetailsViewModel => new List<LotteryDetailsViewModel>
            {
                new LotteryDetailsViewModel() { Id = 1,  Status = 2, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor",  EntryFee = -5 },
                new LotteryDetailsViewModel() { Id = 2,  Status = 2, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", EntryFee = 2 },
                new LotteryDetailsViewModel() { Id = 3,  Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
                new LotteryDetailsViewModel() { Id = 4,  Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
                new LotteryDetailsViewModel() { Id = 5,  Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
                new LotteryDetailsViewModel() { Id = 6,  Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
                new LotteryDetailsViewModel() { Id = 7,  Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
                new LotteryDetailsViewModel() { Id = 8,  Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 },
            };

        private UserAndOrganizationDTO GetUserAndOrganization()
        {
            return new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "1"
            };
        }

    }
}
