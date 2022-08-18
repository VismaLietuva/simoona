using NSubstitute;
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
using Shrooms.Premium.Domain.DomainServiceValidators.Lotteries;
using Shrooms.Premium.Domain.Services.Email.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _sut;
        private IUnitOfWork2 _uow;
        private DbSet<Lottery> _lotteriesDbSet;
        private DbSet<LotteryParticipant> _lotteryParticipantsDbSet;
        private IAsyncRunner _asyncRunner;
        private ILotteryParticipantService _participantService;
        private IUserService _userService;
        private ISystemClock _systemClock;
        private ILotteryValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _systemClock = Substitute.For<ISystemClock>();
            _uow = Substitute.For<IUnitOfWork2>();
            _lotteriesDbSet = _uow.MockDbSetForAsync<Lottery>();
            _lotteryParticipantsDbSet = _uow.MockDbSetForAsync<LotteryParticipant>();
            _uow.GetDbSet<Lottery>().Returns(_lotteriesDbSet);
            _uow.GetDbSet<LotteryParticipant>().Returns(_lotteryParticipantsDbSet);

            _asyncRunner = Substitute.For<IAsyncRunner>();
            _participantService = Substitute.For<ILotteryParticipantService>();
            _userService = Substitute.For<IUserService>();
            _validator = Substitute.For<ILotteryValidator>();
            var kudosService = Substitute.For<IKudosService>();

            _sut = new LotteryService(
                _uow,
                _participantService,
                _userService,
                kudosService,
                _asyncRunner,
                _systemClock,
                _validator);
        }

        [Test]
        public async Task CreateLotteryAsync_CorrectValues_CreatesLottery()
        {
            // Arrange
            var newLotteryDto = new LotteryDto();
            var userOrg = new UserAndOrganizationDto();

            // Act
            await _sut.CreateLotteryAsync(newLotteryDto, userOrg);

            // Assert
            await _uow.Received().SaveChangesAsync(userOrg.UserId);
        }


        [Test]
        public async Task CreateLotteryAsync_CreatedStartedLottery_SendsNotficationEmail()
        {
            // Arrange
            var newLotteryDto = new LotteryDto
            {
                Status = LotteryStatus.Started
            };

            var userOrg = new UserAndOrganizationDto();

            // Act
            await _sut.CreateLotteryAsync(newLotteryDto, userOrg);

            // Assert
            _asyncRunner
                .Received(1)
                .Run(Arg.Any<Func<ILotteryNotificationService, Task>>(), Arg.Any<string>());
        }


        [Test]
        public async Task CreateLotteryAsync_CreatedDraftedLottery_DoesNotSendNotficationEmail()
        {
            // Arrange
            var newLotteryDto = new LotteryDto
            {
                Status = LotteryStatus.Drafted
            };

            var userOrg = new UserAndOrganizationDto();

            // Act
            await _sut.CreateLotteryAsync(newLotteryDto, userOrg);

            // Assert
            _asyncRunner
                .Received(0)
                .Run(Arg.Any<Func<ILotteryNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task EditDraftedLotteryAsync_CorrectValues_EditsLottery()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var lotteryDto = new LotteryDto
            {
                Id = lottery.Id
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = lottery.OrganizationId
            };

            // Act
            await _sut.EditDraftedLotteryAsync(lotteryDto, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<Lottery>());

            _validator
                .Received(1)
                .CheckIfLotteryIsDrafted(Arg.Any<Lottery>());

            await _uow
                .Received(1)
                .SaveChangesAsync(Arg.Any<bool>());
        }

        [Test]
        public async Task EditDraftedLotteryAsync_ChangeStatusToStarted_SendsNotificationEmail()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var lotteryDto = new LotteryDto
            {
                Id = lottery.Id,
                Status = LotteryStatus.Started
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = lottery.OrganizationId
            };

            // Act
            await _sut.EditDraftedLotteryAsync(lotteryDto, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<Lottery>());

            _validator
                .Received(1)
                .CheckIfLotteryIsDrafted(Arg.Any<Lottery>());

            _asyncRunner
                .Received(1)
                .Run(Arg.Any<Func<ILotteryNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task EditDraftedLotteryAsync_ChangeStatusToDrafted_DoesNotSendNotificationEmail()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var lotteryDto = new LotteryDto
            {
                Id = lottery.Id,
                Status = LotteryStatus.Drafted
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = lottery.OrganizationId
            };

            // Act
            await _sut.EditDraftedLotteryAsync(lotteryDto, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<Lottery>());

            _validator
                .Received(1)
                .CheckIfLotteryIsDrafted(Arg.Any<Lottery>());

            _asyncRunner
                .Received(0)
                .Run(Arg.Any<Func<ILotteryNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task EditStartedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var lotteryDto = new LotteryDto
            {
                Id = lottery.Id,
                Status = LotteryStatus.Drafted
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = lottery.OrganizationId
            };

            // Act
            await _sut.EditDraftedLotteryAsync(lotteryDto, userOrg);

            // Assert
            await _uow
                .Received(1)
                .SaveChangesAsync(Arg.Any<bool>());
        }

        [Test]
        public async Task GetLotteryDetailsAsync_WhenLotteryIsNotFound_ReturnsNull()
        {
            // Arrange
            CreateAndAddLotteryWithIdToDbSet(1);

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            // Act
            var actual = await _sut.GetLotteryDetailsAsync(int.MaxValue, false, userOrg);

            // Assert
            Assert.IsNull(actual);
        }

        [Test]
        public async Task GetLotteryDetailsAsync_CorrectValues_SetsCorrectLotteryParticipantCount()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var lotteryParticipants = new List<LotteryParticipant>
            {
                new LotteryParticipant
                {
                    Id = 1,
                    LotteryId = lottery.Id,
                    Lottery = lottery
                },

                new LotteryParticipant
                {
                    Id = 2,
                    LotteryId = lottery.Id,
                    Lottery = lottery
                }
            };

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(lotteryParticipants);

            const int expected = 2;

            // Act
            var actual = await _sut.GetLotteryDetailsAsync(lottery.Id, false, userOrg);

            // Assert
            Assert.AreEqual(expected, actual.Participants);
        }

        [Test]
        public async Task GetLotteryDetailsAsync_WhenIncludingBuyer_SetsBuyerValue()
        {
            // Arrange
            var buyerId = Guid.NewGuid().ToString();
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var lotteryParticipants = new List<LotteryParticipant>
            {
                new LotteryParticipant
                {
                    Id = 1,
                    LotteryId = lottery.Id,
                    Lottery = lottery,
                    CreatedBy = buyerId
                },

                new LotteryParticipant
                {
                    Id = 2,
                    LotteryId = lottery.Id,
                    Lottery = lottery,
                    CreatedBy = buyerId
                }
            };

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(lotteryParticipants);

            _userService
                .GetApplicationUserAsync(Arg.Any<string>())
                .Returns(new ApplicationUser
                {
                    Id = buyerId
                });

            // Act
            var actual = await _sut.GetLotteryDetailsAsync(lottery.Id, true, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfBuyerExists(Arg.Any<ApplicationUser>());

            Assert.NotNull(actual.Buyer);
        }

        [Test]
        public async Task GetLotteryDetailsAsync_WhenIncludingBuyer_SetsCorrectRemainingGiftedTicketCount()
        {
            // Arrange
            var buyerId = Guid.NewGuid().ToString();

            var lottery = new Lottery
            {
                Id = 1,
                OrganizationId = TestConstants.DefaultOrganizationId,
                GiftedTicketLimit = 10
            };

            _lotteriesDbSet.SetDbSetDataForAsync(new List<Lottery> { lottery });

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId,
                UserId = buyerId
            };

            var lotteryParticipants = new List<LotteryParticipant>
            {
                new LotteryParticipant
                {
                    Id = 1,
                    LotteryId = lottery.Id,
                    Lottery = lottery,
                    CreatedBy = buyerId
                },

                new LotteryParticipant
                {
                    Id = 2,
                    LotteryId = lottery.Id,
                    Lottery = lottery,
                    CreatedBy = buyerId
                }
            };

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(lotteryParticipants);

            _userService
                .GetApplicationUserAsync(Arg.Any<string>())
                .Returns(new ApplicationUser
                {
                    Id = buyerId
                });

            const int expected = 8;

            // Act
            var actual = await _sut.GetLotteryDetailsAsync(lottery.Id, true, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfBuyerExists(Arg.Any<ApplicationUser>());

            Assert.AreEqual(expected, actual.Buyer.RemainingGiftedTicketCount);
        }

        [Test]
        public async Task GetLotteryDetailsAsync_WhenNotIncludingBuyer_SetsBuyerToNull()
        {
            // Arrange
            var lottery = CreateAndAddLotteryWithIdToDbSet(1);

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var lotteryParticipants = new List<LotteryParticipant>();

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(lotteryParticipants);

            // Act
            var actual = await _sut.GetLotteryDetailsAsync(lottery.Id, false, userOrg);

            // Assert
            Assert.IsNull(actual.Buyer);
        }

        [Test]
        public async Task AbortLotteryAsync_NonExistentLottery_ReturnsFalse()
        {
            // Arrange
            MockLotteries();

            // Act
            var result = await _sut.AbortLotteryAsync(default, default);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(8)]
        public async Task AbortLotteryAsync_StatusPossibleToAbort_AbortedSuccessfully(int lotteryId)
        {
            // Arrange
            MockLotteries();

            // Act
            var result = await _sut.AbortLotteryAsync(lotteryId, GetUserOrg());

            // Assert
            Assert.AreEqual(expected: true, result);
        }

        [Test]
        public async Task RefundParticipantsAsync_NonExistentLottery_DoesNothing()
        {
            // Arrange
            MockLotteries();

            // Act
            await _sut.RefundParticipantsAsync(default, default);

            // Assert
            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [Test]
        public async Task RefundParticipantsAsync_RefundIsNotFailed_DoesNotStartRefund()
        {
            // Arrange
            MockLotteries();

            // Act
            await _sut.RefundParticipantsAsync(1, GetUserOrg());

            // Assert
            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [TestCase(5)]
        public async Task RefundParticipantsAsync_RefundFailed_StartsNewRefund(int lotteryId)
        {
            // Arrange
            MockLotteries();
            var userOrg = GetUserOrg();

            // Act
            await _sut.RefundParticipantsAsync(lotteryId, userOrg);

            // Assert
            await _uow.Received().SaveChangesAsync(userOrg.UserId);
            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lotteryId, userOrg), default);
        }

        [Test]
        public async Task UpdateRefundFailedFlagAsync_LotteryFound_FlagUpdated()
        {
            // Arrange
            MockLotteries();
            var userOrg = GetUserOrg();

            // Act
            await _sut.UpdateRefundFailedFlagAsync(1, true, userOrg);

            // Assert
            await _uow.Received().SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public async Task FinishLotteryAsync_LotteryFound_ChangesStatus()
        {
            // Arrange
            MockLotteries();
            var userOrg = GetUserOrg();

            // Act
            await _sut.FinishLotteryAsync(1, userOrg);

            // Assert
            await _uow.Received().SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public async Task GetLotteryStatsAsync_LotteryFound_ReturnsCorrectDto()
        {
            // Arrange
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

            // Act
            var result = await _sut.GetLotteryStatsAsync(lotteryId, GetUserOrg());

            // Assert
            Assert.AreEqual(expected.TotalParticipants, result.TotalParticipants);
            Assert.AreEqual(expected.KudosSpent, result.KudosSpent);
            Assert.AreEqual(expected.TicketsSold, result.TicketsSold);
        }

        [Test]
        public async Task GetLotteriesAsync_OrganizationWithLotteries_ReturnsCorrectAmountOfLotteries()
        {
            // Arrange
            MockLotteries();

            // Act
            var result = await _sut.GetLotteriesAsync(new UserAndOrganizationDto { OrganizationId = 12345 });

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetLotteryStatusAsync_ExistingLottery_ReturnsCorrectStatusObject()
        {
            // Arrange
            MockLotteries();
            var expected = new LotteryStatusDto
            {
                LotteryStatus = LotteryStatus.Started,
                RefundFailed = false
            };

            // Act
            var result = await _sut.GetLotteryStatusAsync(1, GetUserOrg());

            // Assert
            Assert.AreEqual(expected.LotteryStatus, result.LotteryStatus);
            Assert.AreEqual(expected.RefundFailed, result.RefundFailed);
        }

        [Test]
        public void BuyLotteryTicketsAsync_WhenBuyingForYourselfAndNegativeTicketCount_ThrowsLotteryException()
        {
            // Arrange
            const int lotteryId = 1;
            const string buyerId = "id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyerUser = new ApplicationUser
            {
                Id = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyTicketsDto = new BuyLotteryTicketsDto
            {
                LotteryId = lotteryId,
                TicketCount = -1,
                Receivers = new LotteryTicketReceiverDto[0]
            };

            var lottery = CreateAndAddLotteryWithIdToDbSet(lotteryId);

            _userService
                .GetApplicationUserAsync(Arg.Is(buyerId))
                .Returns(buyerUser);

            _validator
                .IsValidTicketCount(buyTicketsDto)
                .Returns(false);

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(new List<LotteryParticipant>());

            // Assert
            Assert.ThrowsAsync<LotteryException>(async () => await _sut.BuyLotteryTicketsAsync(buyTicketsDto, userOrg));

            _validator
                .Received(1)
                .CheckIfBuyerExists(Arg.Is(buyerUser));

            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfLotteryEnded(Arg.Any<LotteryDetailsDto>());
        }

        [Test]
        public async Task BuyLotteryTicketsAsync_WhenBuyingForYourself_BuysTickets()
        {
            // Arrange
            const int lotteryId = 1;
            const string buyerId = "id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyerUser = new ApplicationUser
            {
                Id = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyTicketsDto = new BuyLotteryTicketsDto
            {
                LotteryId = lotteryId,
                TicketCount = 10,
                Receivers = new LotteryTicketReceiverDto[0]
            };

            CreateAndAddLotteryWithIdToDbSet(lotteryId);

            _userService
                .GetApplicationUserAsync(Arg.Is(buyerId))
                .Returns(buyerUser);

            _validator
                .IsValidTicketCount(buyTicketsDto)
                .Returns(true);

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(new List<LotteryParticipant>());

            // Act
            await _sut.BuyLotteryTicketsAsync(buyTicketsDto, userOrg);

            // Assert
            _validator
                .Received(1)
                .CheckIfBuyerExists(Arg.Is(buyerUser));

            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfLotteryEnded(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfUserHasEnoughKudos(Arg.Is(buyerUser), Arg.Any<int>());

            await _uow
                .Received(1)
                .SaveChangesAsync(Arg.Is(buyerUser.Id));
        }

        [Test]
        public async Task BuyLotteryTicketsAsync_WhenGifting_BuysTickets()
        {
            // Arrange
            const int lotteryId = 1;
            const string buyerId = "id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyerUser = new ApplicationUser
            {
                Id = buyerId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var buyTicketsDto = new BuyLotteryTicketsDto
            {
                LotteryId = lotteryId,
                TicketCount = 10,
                Receivers = new LotteryTicketReceiverDto[]
                {
                    new LotteryTicketReceiverDto
                    {
                        UserId = "id2",
                        TicketCount = 10
                    }
                }
            };

            CreateAndAddLotteryWithIdToDbSet(lotteryId);

            _userService
                .GetApplicationUserAsync(Arg.Is(buyerId))
                .Returns(buyerUser);

            _lotteryParticipantsDbSet.SetDbSetDataForAsync(new List<LotteryParticipant>());

            // Act
            await _sut.BuyLotteryTicketsAsync(buyTicketsDto, userOrg);

            // Assertmedia
            _validator
                .Received(2)
                .CheckIfBuyerExists(Arg.Is(buyerUser));

            _validator
                .Received(1)
                .CheckIfLotteryExists(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfLotteryEnded(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfLotteryAllowsGifting(Arg.Any<LotteryDetailsDto>());

            _validator
                .Received(1)
                .CheckIfGiftedTicketLimitIsExceeded(Arg.Any<LotteryDetailsBuyerDto>(), Arg.Any<int>());

            await _validator
                .Received(1)
                .CheckIfGiftReceiversExistAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<UserAndOrganizationDto>());

            _validator
                .Received(1)
                .CheckIfUserHasEnoughKudos(Arg.Is(buyerUser), Arg.Any<int>());

            await _uow
                .Received(1)
                .SaveChangesAsync(Arg.Is(buyerUser.Id));
        }

        [Test]
        public async Task GetPagedLotteriesAsync_WhenFilterIsNotPresent_ReturnsAllLotteries()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var args = new LotteryListingArgsDto
            {
                Filter = null,
                SortByProperties = null,
                PageSize = 10,
                Page = 1
            };

            MockLotteries();

            var expectedCount = 5;

            // Act
            var result = await _sut.GetPagedLotteriesAsync(args, userOrg);

            // Assert
            Assert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public async Task GetPagedLotteriesAsync_WhenFilterIsPresent_ReturnsLotteriesThatContainFilter()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var args = new LotteryListingArgsDto
            {
                Filter = "foobar",
                SortByProperties = null,
                PageSize = 10,
                Page = 1
            };

            MockLotteries();

            // Act
            var result = await _sut.GetPagedLotteriesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<LotteryDetailsDto>(lottery => lottery.Title.Contains(args.Filter)));
        }

        [Test]
        public async Task GetPagedLotteriesAsync_WhenCollectionContainsLotteriesThatFailedRefund_OrdersFailedRefundLotteriesFirst()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var args = new LotteryListingArgsDto
            {
                Filter = null,
                SortByProperties = null,
                PageSize = 10,
                Page = 1
            };

            MockLotteries();

            // Act
            var result = await _sut.GetPagedLotteriesAsync(args, userOrg);

            // Assert
            Assert.IsTrue(result.FirstOrDefault().RefundFailed);
        }

        [Test]
        public async Task GetRunningLotteries_OrganizationHasLotteries_ReturnsLotteries()
        {
            MockLotteries();

            var result = await _sut.GetRunningLotteriesAsync(GetUserOrg());

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetLotteryByIdAsync_ValidId_ReturnsLottery()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            const int expectedId = 1;

            CreateAndAddLotteryWithIdToDbSet(expectedId);

            // Act
            var actual = await _sut.GetLotteryByIdAsync(expectedId, userOrg);

            // Assert
            Assert.AreEqual(actual.Id, expectedId);
        }

        [Test]
        public async Task GetLotteryByIdAsync_NotValidId_ReturnsNull()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            _lotteriesDbSet.SetDbSetDataForAsync(new List<Lottery>());

            // Act
            var actual = await _sut.GetLotteryByIdAsync(1, userOrg);

            // Assert
            Assert.IsNull(actual);
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

            _lotteriesDbSet.SetDbSetDataForAsync(data.AsQueryable());
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
                    Status = LotteryStatus.Started,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 2,
                    Title = "foobar",
                    OrganizationId = 2,
                    Status = LotteryStatus.Drafted,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 3,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 3,
                    Title = "barfoo",
                    OrganizationId = 1,
                    Status = LotteryStatus.Ended,
                    EndDate = DateTime.Now.AddDays(-2),
                    EntryFee = 1,
                    IsRefundFailed = false
                },
                new Lottery
                {
                    Id = 4,
                    OrganizationId = 1,
                    Title = string.Empty,
                    Status = LotteryStatus.Drafted,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1
                },
                new Lottery
                {
                    Id = 5,
                    Status = LotteryStatus.Started,
                    OrganizationId = 1,
                    Title = string.Empty,
                    IsRefundFailed = true,
                    EndDate = DateTime.Now.AddDays(2)
                },
                new Lottery
                {
                    Id = 6,
                    Status = LotteryStatus.Ended,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(2)
                },
                new Lottery
                {
                    Id = 7,
                    Status = LotteryStatus.Ended,
                    IsRefundFailed = true,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(3)
                },
                new Lottery
                {
                    Id = 8,
                    Title = "foo",
                    OrganizationId = 1,
                    Status = LotteryStatus.Expired,
                    EndDate = DateTime.Now.AddDays(2),
                    EntryFee = 1,
                    IsRefundFailed = false
                }
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

        private Lottery CreateAndAddLotteryWithIdToDbSet(int id)
        {
            var lottery = new Lottery
            {
                Id = id,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            _lotteriesDbSet.SetDbSetDataForAsync(new List<Lottery> { lottery });

            return lottery;
        }
    }
}
