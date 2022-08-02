using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.Api.Controllers.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Tests.Extensions;
using X.PagedList;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class LotteryControllerTests
    {
        private LotteryController _lotteryController;

        private IMapper _mapper;
        private ILotteryService _lotteryService;
        private ILotteryExportService _lotteryExportService;

        [SetUp]
        public void TestInitializers()
        {
            _mapper = Substitute.For<IMapper>();
            _lotteryService = Substitute.For<ILotteryService>();
            _lotteryExportService = Substitute.For<ILotteryExportService>();

            _lotteryController = new LotteryController(_mapper, _lotteryService, _lotteryExportService);
            _lotteryController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetAllLotteries_Should_Return_Ok_With_IEnumerable_Of_LotteryDetails_ViewModel()
        {
            // Arrange
            _mapper.Map<IEnumerable<LotteryDetailsDto>, IEnumerable<LotteryDetailsViewModel>>(LotteryDetailsDto)
                .Returns(LotteryDetailsViewModel);

            _lotteryService.GetLotteriesAsync(UserAndOrganizationArg).Returns(LotteryDetailsDto);

            // Act
            var response = await _lotteryController.GetAllLotteries();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<LotteryDetailsViewModel>>>(response);
            await _lotteryService.Received(1).GetLotteriesAsync(UserAndOrganizationArg);
        }

        [Test]
        public async Task GetLottery_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new LotteryDetailsViewModel
            {
                Id = 2,
                Status = 1,
                Title = "Hello"
            };

            var lotteryDto = new LotteryDetailsDto
            {
                Id = 2,
                Status = 1,
                Title = "Hello"
            };

            _mapper.Map<LotteryDetailsDto, LotteryDetailsViewModel>(lotteryDto).Returns(lotteryViewModel);

            _lotteryService.GetLotteryDetailsAsync(2, UserAndOrganizationArg).Returns(lotteryDto);

            // Act
            var response = await _lotteryController.GetLottery(2);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryDetailsViewModel>>(response);
            await _lotteryService.Received(1).GetLotteryDetailsAsync(2, UserAndOrganizationArg);
        }

        [Test]
        public async Task GetLottery_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.GetLotteryDetailsAsync(3000, UserAndOrganizationArg).Returns((LotteryDetailsDto)null);

            // Act
            var response = await _lotteryController.GetLottery(3000);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            await _lotteryService.Received(1).GetLotteryDetailsAsync(3000, UserAndOrganizationArg);
        }

        [Test]
        public async Task Abort_Should_Return_Ok()
        {
            // Arrange
            _lotteryService.AbortLotteryAsync(2, UserAndOrganizationArg).Returns(true);

            // Act
            var response = await _lotteryController.Abort(2);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).AbortLotteryAsync(2, UserAndOrganizationArg);
        }

        [Test]
        public async Task Abort_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.AbortLotteryAsync(5, UserAndOrganizationArg).Returns(false);

            // Act
            var response = await _lotteryController.Abort(5);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            await _lotteryService.Received(1).AbortLotteryAsync(5, UserAndOrganizationArg);
        }

        [Test]
        public async Task CreateLottery_Should_Return_Invalid_Model_State()
        {
            // Arrange
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };

            var lotteryDto = new LotteryDto
            {
                Title = "test"
            };

            _mapper.Map<CreateLotteryViewModel, LotteryDto>(lotteryViewModel).Returns(lotteryDto);

            // Act
            _lotteryController.ModelState.AddModelError("model", "error");

            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsInstanceOf<InvalidModelStateResult>(response);
            await _lotteryService.DidNotReceive().CreateLotteryAsync(lotteryDto, new UserAndOrganizationDto());
        }

        [Test]
        public async Task CreateLottery_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };

            var lotteryDto = new LotteryDto
            {
                Title = "test"
            };

            _mapper.Map<CreateLotteryViewModel, LotteryDto>(lotteryViewModel).Returns(lotteryDto);

            // Act
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).CreateLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task CreateLottery_Should_Return_Bad_Request()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };

            var lotteryDto = new LotteryDto
            {
                Title = "test"
            };

            _mapper.Map<CreateLotteryViewModel, LotteryDto>(lotteryViewModel).Returns(lotteryDto);
            _lotteryService.CreateLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>()).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).CreateLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task BuyLotteryTicket_Should_Return_Ok()
        {
            // Arrange
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };

            var ticketDto = new BuyLotteryTicketDto
            {
                LotteryId = 1,
                Tickets = 5
            };

            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDto>(ticketViewModel).Returns(ticketDto);

            // Act
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDto, UserAndOrganizationArg);
        }

        [Test]
        public async Task BuyLotteryTicket_Should_Return_Bad_Request()
        {
            // Arrange
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };

            var ticketDto = new BuyLotteryTicketDto
            {
                LotteryId = 1,
                Tickets = 5
            };

            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDto>(ticketViewModel).Returns(ticketDto);

            _lotteryService.BuyLotteryTicketAsync(ticketDto, UserAndOrganizationArg).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDto, UserAndOrganizationArg);
        }

        [Test]
        public async Task GetPagedLotteries_Should_Return_Ok()
        {
            // Arrange
            var args = new GetPagedLotteriesArgs
            {
                Filter = "",
                PageNumber = 1,
                PageSize = 10,
                UserOrg = _userAndOrganization
            };

            var pagedListAsync = await LotteryDetailsDto.ToPagedListAsync(args.PageNumber, args.PageSize);
            _lotteryService.GetPagedLotteriesAsync(args).Returns(pagedListAsync);

            // Act
            var response = await _lotteryController.GetPagedLotteries("", 1, 10);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedViewModel<LotteryDetailsDto>>>(response);
            await _lotteryService.Received(1).GetPagedLotteriesAsync(Arg.Any<GetPagedLotteriesArgs>());
        }

        [Test]
        public async Task RefundParticipants_Should_Return_Ok()
        {
            // Arrange

            // Act
            var response = await _lotteryController.RefundParticipants(1337);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).RefundParticipantsAsync(1337, UserAndOrganizationArg);
        }

        [Test]
        public async Task FinishLottery_Should_Return_Ok()
        {
            // Arrange

            // Act
            var response = await _lotteryController.FinishLottery(37);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37, UserAndOrganizationArg);
        }

        [Test]
        public async Task FinishLottery_Should_Return_Bad_Request()
        {
            // Arrange
            _lotteryService.FinishLotteryAsync(37, UserAndOrganizationArg).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.FinishLottery(37);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37, UserAndOrganizationArg);
        }

        [Test]
        public async Task UpdateDrafted_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new LotteryDto
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, LotteryDto>(lotteryViewModel)
                .Returns(lotteryDto);

            // Act
            var response = await _lotteryController.UpdateDrafted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).EditDraftedLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task UpdateDrafted_Should_Return_Bad_Request()
        {
            // Arrange
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new LotteryDto
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, LotteryDto>(lotteryViewModel)
                .Returns(lotteryDto);

            _lotteryService.When(x => x.EditDraftedLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>()))
                .Do(_ => throw new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.UpdateDrafted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).EditDraftedLotteryAsync(lotteryDto, Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task UpdateStarted_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31
            };

            var lotteryDto = new EditStartedLotteryDto
            {
                Id = 31
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDto>(lotteryViewModel)
                .Returns(lotteryDto);

            // Act
            var response = await _lotteryController.UpdateStarted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).EditStartedLotteryAsync(lotteryDto);
        }

        [Test]
        public async Task UpdateStarted_Should_Return_Bad_Request()
        {
            // Arrange
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31
            };

            var lotteryDto = new EditStartedLotteryDto
            {
                Id = 31
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDto>(lotteryViewModel)
                .Returns(lotteryDto);
            _lotteryService.When(x => x.EditStartedLotteryAsync(lotteryDto))
                .Do(_ => throw new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.UpdateStarted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).EditStartedLotteryAsync(lotteryDto);
        }

        [Test]
        public async Task LotteryStats_Should_Return_Ok()
        {
            // Arrange
            var lotteryStats = new LotteryStatsDto
            {
                KudosSpent = 60,
                TicketsSold = 30,
                TotalParticipants = 15
            };

            _lotteryService.GetLotteryStatsAsync(13, UserAndOrganizationArg).Returns(lotteryStats);

            // Act
            var response = await _lotteryController.LotteryStats(13);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryStatsDto>>(response);
            await _lotteryService.Received(1).GetLotteryStatsAsync(13, UserAndOrganizationArg);
        }

        [Test]
        public async Task LotteryStats_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.GetLotteryStatsAsync(13, UserAndOrganizationArg).Returns((LotteryStatsDto)null);

            // Act
            var response = await _lotteryController.LotteryStats(13);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            await _lotteryService.Received(1).GetLotteryStatsAsync(13, UserAndOrganizationArg);
        }

        private IEnumerable<LotteryDetailsDto> LotteryDetailsDto => new List<LotteryDetailsDto>
        {
            new LotteryDetailsDto { Id = 1, Status = 2, EndDate = DateTime.Now.AddDays(2), Title = "Monitor", EntryFee = -5 },
            new LotteryDetailsDto { Id = 2, Status = 2, EndDate = DateTime.Now.AddDays(-5), Title = "Computer", EntryFee = 2 },
            new LotteryDetailsDto { Id = 3, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
            new LotteryDetailsDto { Id = 4, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
            new LotteryDetailsDto { Id = 5, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
            new LotteryDetailsDto { Id = 6, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
            new LotteryDetailsDto { Id = 7, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
            new LotteryDetailsDto { Id = 8, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 }
        };

        private IEnumerable<LotteryDetailsViewModel> LotteryDetailsViewModel => new List<LotteryDetailsViewModel>
        {
            new LotteryDetailsViewModel { Id = 1, Status = 2, EndDate = DateTime.Now.AddDays(2), Title = "Monitor", EntryFee = -5 },
            new LotteryDetailsViewModel { Id = 2, Status = 2, EndDate = DateTime.Now.AddDays(-5), Title = "Computer", EntryFee = 2 },
            new LotteryDetailsViewModel { Id = 3, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
            new LotteryDetailsViewModel { Id = 4, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 5, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 6, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 7, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 8, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 }
        };

        private readonly UserAndOrganizationDto _userAndOrganization = new UserAndOrganizationDto
        {
            OrganizationId = 1,
            UserId = "1"
        };

        private UserAndOrganizationDto UserAndOrganizationArg =>
            Arg.Is<UserAndOrganizationDto>(o => o.UserId == _userAndOrganization.UserId && o.OrganizationId == _userAndOrganization.OrganizationId);
    }
}
