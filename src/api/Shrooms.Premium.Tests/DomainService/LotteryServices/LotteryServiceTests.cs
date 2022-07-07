using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _sut;
        private IUnitOfWork2 _unitOfWork;
        private DbSet<Lottery> _lotteriesDb;
        private DbSet<LotteryParticipant> _lotteryParticipantsDb;
        private IMapper _mapper;
        private IAsyncRunner _asyncRunner;
        private IParticipantService _participantService;
        private IUserService _userService;
        private ISystemClock _systemClock;

        [SetUp]
        public void SetUp()
        {
            _systemClock = Substitute.For<ISystemClock>();
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDb = _unitOfWork.MockDbSetForAsync<Lottery>();
            _lotteryParticipantsDb = _unitOfWork.MockDbSetForAsync<LotteryParticipant>();
            _unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDb);
            _unitOfWork.GetDbSet<LotteryParticipant>().Returns(_lotteryParticipantsDb);

            _asyncRunner = Substitute.For<IAsyncRunner>();
            _participantService = Substitute.For<IParticipantService>();
            _userService = Substitute.For<IUserService>();
            var kudosService = Substitute.For<IKudosService>();
            _mapper = Substitute.For<IMapper>();

            _sut = new LotteryService(_unitOfWork, _mapper, _participantService, _userService, kudosService, _asyncRunner, _systemClock);
        }

        [TestCase(0, "Invalid entry fee.")]
        [TestCase(1, "Lottery can't start in the past.")]
        [TestCase(2, "Invalid status of created lottery.")]
        [TestCase(5, "Invalid status of created lottery.")]
        [TestCase(6, "Invalid entry fee.")]
        public void CreateLottery_IncorrectFieldValues_ThrowsException(int index, string message)
        {
            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            var lotteryDto = GetCreateLotteryDtoList()[index];

            var result = Assert.ThrowsAsync<LotteryException>(async () => await _sut.CreateLotteryAsync(lotteryDto));

            Assert.That(result.Message, Is.EqualTo(message));
        }

        [Test]
        public async Task CreateLottery_CorrectLotteryDto_CreatesLottery()
        {
            var lotteryDto = GetCreateLotteryDtoList()[3];
            _mapper.Map<LotteryDto, Lottery>(lotteryDto).Returns(GetLottery());

            await _sut.CreateLotteryAsync(lotteryDto);

            _lotteriesDb.ReceivedWithAnyArgs().Add(Arg.Any<Lottery>());
            await _unitOfWork.Received().SaveChangesAsync(lotteryDto.UserId);
        }

        [Test]
        public void EditDraftedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.FindAsync().ReturnsForAnyArgs(GetLottery());

            var result = Assert.ThrowsAsync<LotteryException>(async () => await _sut.EditDraftedLotteryAsync(new LotteryDto()));

            Assert.That(result.Message, Is.EqualTo("Editing is forbidden for not drafted lottery."));
        }

        [Test]
        public async Task EditDraftedLottery_CorrectLotteryDto_EditsLotterySuccessfully()
        {
            _lotteriesDb.FindAsync().ReturnsForAnyArgs(GetLottery(LotteryStatus.Drafted));

            await _sut.EditDraftedLotteryAsync(new LotteryDto());

            await _unitOfWork.Received().SaveChangesAsync(false);
        }

        [Test]
        public void EditDraftedLottery_EndDateInThePast_ThrowsException()
        {
            // Arrange
            _lotteriesDb.FindAsync().ReturnsForAnyArgs(GetLottery(LotteryStatus.Drafted));
            _systemClock.UtcNow.Returns(DateTime.UtcNow.AddDays(-1000));

            // Assert
            Assert.ThrowsAsync<LotteryException>(async () => await _sut.EditDraftedLotteryAsync(new LotteryDto()));
        }

        [Test]
        public void EditStartedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.FindAsync().ReturnsForAnyArgs(GetLottery(LotteryStatus.Refunded));

            var result = Assert.ThrowsAsync<LotteryException>(async () => await _sut.EditStartedLotteryAsync(new EditStartedLotteryDto()));

            Assert.That(result.Message, Is.EqualTo("Lottery is not running."));
        }

        [Test]
        public async Task EditStartedLottery_CorrectLotteryDto_EditsLotterySuccessfully()
        {
            _lotteriesDb.FindAsync().ReturnsForAnyArgs(GetLottery());

            await _sut.EditStartedLotteryAsync(new EditStartedLotteryDto());

            await _unitOfWork.Received().SaveChangesAsync(false);
        }

        [Test]
        public async Task GetLotteryDetails_NonExistentLottery_ReturnsNull()
        {
            MockLotteries();

            var result = await _sut.GetLotteryDetailsAsync(default, default);

            Assert.IsNull(result);
        }

        [Test]
        public async Task AbortLottery_NonExistentLottery_ReturnsFalse()
        {
            MockLotteries();

            var result = await _sut.AbortLotteryAsync(default, default);

            Assert.AreEqual(false, result);
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(8)]
        public async Task AbortLottery_StatusPossibleToAbort_AbortedSuccessfully(int lotteryId)
        {
            MockLotteries();

            var result = await _sut.AbortLotteryAsync(lotteryId, GetUserOrg());

            Assert.AreEqual(expected: true, result);
        }

        [Test]
        public async Task RefundParticipants_NonExistentLottery_DoesNothing()
        {
            MockLotteries();

            await _sut.RefundParticipantsAsync(default, default);

            await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [Test]
        public async Task RefundsParticipants_RefundIsNotFailed_DoesNotStartRefund()
        {
            MockLotteries();

            await _sut.RefundParticipantsAsync(1, GetUserOrg());

            await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [TestCase(5)]
        public async Task RefundParticipants_RefundFailed_StartsNewRefund(int lotteryId)
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            await _sut.RefundParticipantsAsync(lotteryId, userOrg);

            await _unitOfWork.Received().SaveChangesAsync(userOrg.UserId);
            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lotteryId, userOrg), default);
        }

        [Test]
        public async Task UpdateRefundFailedFlag_LotteryFound_FlagUpdated()
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            await _sut.UpdateRefundFailedFlagAsync(1, true, userOrg);

            await _unitOfWork.Received().SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public async Task FinishLotteryAsync_LotteryFound_ChangesStatus()
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            await _sut.FinishLotteryAsync(1, userOrg);

            await _unitOfWork.Received().SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public async Task GetLotteryStats_LotteryFound_ReturnsCorrectDto()
        {
            MockLotteries();
            const int lotteryId = 1;

            var participantDto = GetParticipantDto();
            _participantService.GetParticipantsCountedAsync(lotteryId).Returns(participantDto);

            var expected = new LotteryStatsDto
            {
                TotalParticipants = 4,
                KudosSpent = 10,
                TicketsSold = 10
            };

            var result = await _sut.GetLotteryStatsAsync(lotteryId, GetUserOrg());

            Assert.AreEqual(expected.TotalParticipants, result.TotalParticipants);
            Assert.AreEqual(expected.KudosSpent, result.KudosSpent);
            Assert.AreEqual(expected.TicketsSold, result.TicketsSold);
        }

        [Test]
        public async Task GetLotteries_OrganizationWithLotteries_ReturnsCorrectAmountOfLotteries()
        {
            MockLotteries();

            var result = await _sut.GetLotteriesAsync(new UserAndOrganizationDto { OrganizationId = 12345 });

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetFilteredLotteries_FilterSet_ReturnsCorrectAmountOfLotteries()
        {
            MockLotteries();

            var result = await _sut.GetFilteredLotteriesAsync("foo", GetUserOrg());

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public async Task GetLotteryStatus_ExistingLottery_ReturnsCorrectStatusObject()
        {
            MockLotteries();
            var expected = new LotteryStatusDto
            {
                LotteryStatus = (int)LotteryStatus.Started,
                RefundFailed = false
            };

            var result = await _sut.GetLotteryStatusAsync(1, GetUserOrg());

            Assert.AreEqual(expected.LotteryStatus, result.LotteryStatus);
            Assert.AreEqual(expected.RefundFailed, result.RefundFailed);
        }

        [Test]
        public async Task BuyLotteryTicketAsync_NonExistentUser_StopsExecuting()
        {
            MockLotteries();
            _userService.GetApplicationUserAsync("1").ReturnsNullForAnyArgs();

            await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDto { LotteryId = 1 }, new UserAndOrganizationDto { UserId = "1" });

            await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync((string)default);
        }

        [Test]
        public async Task BuyLotteryTicketAsync_NonExistentLottery_StopsExecuting()
        {
            MockLotteries();
            _userService.GetApplicationUserAsync(default).ReturnsForAnyArgs(new ApplicationUser());

            await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDto { LotteryId = int.MaxValue }, GetUserOrg());

            await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync((string)default);
        }

        [Test]
        public void BuyLotteryTicketAsync_NotEnoughKudos_ThrowsException()
        {
            MockParticipants();
            MockLotteries();

            const int lotteryId = 1;

            _mapper.Map<Lottery, LotteryDetailsDto>(default).ReturnsForAnyArgs(new LotteryDetailsDto { EntryFee = 1 });
            _userService.GetApplicationUserAsync(default).ReturnsForAnyArgs(new ApplicationUser { RemainingKudos = 0 });

            var ex = Assert.ThrowsAsync<LotteryException>(async () => await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDto { LotteryId = lotteryId, Tickets = 10 }, GetUserOrg()));

            Assert.AreEqual("User does not have enough kudos for the purchase.", ex.Message);
        }

        [Test]
        public async Task BuyLotteryTicketAsync_LotteryAlreadyEnded_ThrowsException()
        {
            // Arrange
            _systemClock.UtcNow.Returns(DateTime.UtcNow.AddDays(-1000));

            MockParticipants();
            MockLotteries();

            var lotteryId = 1;

            _mapper.Map<Lottery, LotteryDetailsDto>(default).ReturnsForAnyArgs(new LotteryDetailsDto { EntryFee = 1, EndDate = DateTime.UtcNow.AddDays(10) });
            _userService.GetApplicationUserAsync(default).ReturnsForAnyArgs(new ApplicationUser { RemainingKudos = 100 });

            // Act
            await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDto { LotteryId = lotteryId, Tickets = 10 }, GetUserOrg());

            // Assert
            await _unitOfWork.ReceivedWithAnyArgs().SaveChangesAsync((string)default);
        }

        [TestCase(1)]
        public async Task BuyLotteryTicketAsync_AbleToBuyTickets_BuysSuccessfully(int lotteryId)
        {
            MockParticipants();
            MockLotteries();
            _mapper.Map<Lottery, LotteryDetailsDto>(default).ReturnsForAnyArgs(new LotteryDetailsDto { EntryFee = 1, EndDate = DateTime.UtcNow.AddDays(10) });
            _userService.GetApplicationUserAsync(default).ReturnsForAnyArgs(new ApplicationUser { RemainingKudos = 100 });

            await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDto { LotteryId = lotteryId, Tickets = 10 }, GetUserOrg());

            await _unitOfWork.ReceivedWithAnyArgs().SaveChangesAsync((string)default);
        }

        [Test]
        public async Task GetRunningLotteries_OrganizationHasLotteries_ReturnsLotteries()
        {
            MockLotteries();

            var result = await _sut.GetRunningLotteriesAsync(GetUserOrg());

            Assert.AreEqual(2, result.Count);
        }

        private static IList<LotteryDto> GetCreateLotteryDtoList()
        {
            return new List<LotteryDto>
            {
                new LotteryDto { Id = 1, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(2), Title = "Monitor", UserId = "5", EntryFee = -5 },
                new LotteryDto { Id = 2, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(-5), Title = "Computer", UserId = "5", EntryFee = 2 },
                new LotteryDto { Id = 3, OrganizationId = 1, Status = (int)LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(4), Title = "Table", UserId = "5", EntryFee = 2 },
                new LotteryDto { Id = 4, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDto { Id = 5, OrganizationId = 1, Status = (int)LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDto { Id = 6, OrganizationId = 1, Status = (int)LotteryStatus.Finished, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDto { Id = 7, OrganizationId = 1, Status = (int)LotteryStatus.Finished, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = -5 }
            };
        }

        private IList<LotteryParticipantDto> GetParticipantDto()
        {
            return new List<LotteryParticipantDto>
            {
                new LotteryParticipantDto
                {
                    Tickets = 2
                },
                new LotteryParticipantDto
                {
                    Tickets = 3
                },
                new LotteryParticipantDto
                {
                    Tickets = 2
                },
                new LotteryParticipantDto
                {
                    Tickets = 3
                }
            };
        }

        private void MockLotteries()
        {
            var data = GetLotteries();

            _lotteriesDb.SetDbSetDataForAsync(data.AsQueryable());
        }

        private static IList<Lottery> GetLotteries()
        {
            return new List<Lottery>
            {
                new Lottery
                {
                    Id = 1,
                    Title = "foo",
                    OrganizationId = 1,
                    Status = (int)LotteryStatus.Started,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 2,
                    Title = "foobar",
                    OrganizationId = 2,
                    Status = (int)LotteryStatus.Drafted,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 3,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 3,
                    Title = "barfoo",
                    OrganizationId = 1,
                    Status = (int)LotteryStatus.Finished,
                    EndDate = DateTime.Now.AddDays(-2),
                    EntryFee = 1,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 4,
                    OrganizationId = 1,
                    Title = string.Empty,
                    Status = (int)LotteryStatus.Drafted,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1
                },
                new Lottery
                {
                    Id = 5,
                    Status = (int)LotteryStatus.Started,
                    OrganizationId = 1,
                    Title = string.Empty,
                    IsRefundFailed = true,
                    EndDate = DateTime.Now.AddDays(2)
                },
                new Lottery
                {
                    Id = 6,
                    Status = (int)LotteryStatus.Finished,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(2)
                },
                new Lottery
                {
                    Id = 7,
                    Status = (int)LotteryStatus.Finished,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(3)
                },
                new Lottery
                {
                    Id = 8,
                    Title = "foo",
                    OrganizationId = 1,
                    Status = (int)LotteryStatus.Ended,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1,
                    IsRefundFailed = false
                },
            };
        }

        private void MockParticipants()
        {
            var data = GetParticipants();

            _lotteryParticipantsDb.SetDbSetDataForAsync(data.AsQueryable());
        }

        private static IList<LotteryParticipant> GetParticipants()
        {
            return new List<LotteryParticipant>
            {
                new LotteryParticipant
                {
                    LotteryId = 1
                },
                new LotteryParticipant
                {
                    LotteryId = 2
                }
            };
        }

        private static Lottery GetLottery(LotteryStatus status = LotteryStatus.Started)
        {
            return new Lottery
            {
                Id = 4,
                OrganizationId = 1,
                Status = (int)status,
                EndDate = DateTime.UtcNow.AddDays(5),
                Title = "1000 kudos",
                EntryFee = 5,
                IsRefundFailed = false
            };
        }

        private static UserAndOrganizationDto GetUserOrg()
        {
            return new UserAndOrganizationDto
            {
                UserId = "5",
                OrganizationId = 1
            };
        }
    }
}
