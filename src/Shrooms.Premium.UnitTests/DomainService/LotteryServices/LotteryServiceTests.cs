using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Lottery;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Enums;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries;
using Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Lotteries;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _sut;
        private IUnitOfWork2 _unitOfWork;
        private IDbSet<Lottery> _lotteriesDb;
        private IDbSet<LotteryParticipant> _lotteryParticipantsDb;
        private IMapper _mapper;
        private IAsyncRunner _asyncRunner;
        private IParticipantService _participantService;
        private IUserService _userService;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDb = _unitOfWork.MockDbSet<Lottery>();
            _lotteryParticipantsDb = _unitOfWork.MockDbSet<LotteryParticipant>();
            _unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDb);
            _unitOfWork.GetDbSet<LotteryParticipant>().Returns(_lotteryParticipantsDb);

            _asyncRunner = Substitute.For<IAsyncRunner>();
            _participantService = Substitute.For<IParticipantService>();
            _userService = Substitute.For<IUserService>();
            var kudosService = Substitute.For<IKudosService>();
            _mapper = Substitute.For<IMapper>();

            _sut = new LotteryService(_unitOfWork, _mapper, _participantService, _userService, kudosService, _asyncRunner);
        }

        [TestCase(0, "Invalid entry fee.")]
        [TestCase(1, "Lottery can't start in the past.")]
        [TestCase(2, "Invalid status of created lottery.")]
        [TestCase(5, "Invalid status of created lottery.")]
        [TestCase(6, "Invalid entry fee.")]
        public void CreateLottery_IncorrectFieldValues_ThrowsException(int index, string message)
        {
            var lotteryDTO = GetCreateLotteryDTOList()[index];

            var result = Assert.ThrowsAsync<LotteryException>(async () => await _sut.CreateLottery(lotteryDTO));

            Assert.That(result.Message, Is.EqualTo(message));
        }

        [Test]
        public void CreateLottery_CorrectLotteryDTO_CreatesLottery()
        {
            var lotteryDTO = GetCreateLotteryDTOList()[3];
            _mapper.Map<LotteryDTO, Lottery>(lotteryDTO).Returns(GetLottery());

            _sut.CreateLottery(lotteryDTO);

            _lotteriesDb.ReceivedWithAnyArgs().Add(default);
            _unitOfWork.Received().SaveChangesAsync(lotteryDTO.UserId);
        }

        [Test]
        public void EditDraftedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            var result = Assert.Throws<LotteryException>(() =>
                _sut.EditDraftedLottery(new LotteryDTO()));

            Assert.That(result.Message, Is.EqualTo("Editing is forbidden for not drafted lottery."));
        }

        [Test]
        public void EditDraftedLottery_CorrectLotteryDTO_EditsLotterySuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery(LotteryStatus.Drafted));

            _sut.EditDraftedLottery(new LotteryDTO());

            _unitOfWork.Received().SaveChanges(false);
        }

        [Test]
        public void EditStartedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery(LotteryStatus.Refunded));

            var result = Assert.Throws<LotteryException>(() =>
                _sut.EditStartedLottery(new EditStartedLotteryDTO()));

            Assert.That(result.Message, Is.EqualTo("Lottery is not running."));
        }

        [Test]
        public void EditStartedLottery_CorrectLotteryDTO_EditsLotterySuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            _sut.EditStartedLottery(new EditStartedLotteryDTO());

            _unitOfWork.Received().SaveChanges(false);
        }

        [Test]
        public void GetLotteryDetails_NonExistentLottery_ReturnsNull()
        {
            MockLotteries();

            var result = _sut.GetLotteryDetails(default, default);

            Assert.IsNull(result);
        }

        [Test]
        public void AbortLottery_NonExistentLottery_ReturnsFalse()
        {
            MockLotteries();

            var result = _sut.AbortLottery(default, default);

            Assert.AreEqual(false, result);
        }

        [TestCase(1)]
        [TestCase(4)]
        public void AbortLottery_StatusPossibleToAbort_AbortedSuccessfully(int lotteryId)
        {
            MockLotteries();

            var result = _sut.AbortLottery(lotteryId, GetUserOrg());

            Assert.AreEqual(expected: true, result);
        }

        [Test]
        public void RefundParticipants_NonExistentLottery_DoesNothing()
        {
            MockLotteries();

            _sut.RefundParticipants(default, default);

            _unitOfWork.DidNotReceiveWithAnyArgs().SaveChanges(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [Test]
        public void RefundsParticipants_RefundIsNotFailed_DoesNotStartRefund()
        {
            MockLotteries();

            _sut.RefundParticipants(1, GetUserOrg());

            _unitOfWork.DidNotReceiveWithAnyArgs().SaveChanges(string.Empty);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<ILotteryAbortJob>(default, default);
        }

        [TestCase(5)]
        public void RefundParticipants_RefundFailed_StartsNewRefund(int lotteryId)
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            _sut.RefundParticipants(lotteryId, userOrg);

            _unitOfWork.Received().SaveChanges(userOrg.UserId);
            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryAbortJob>(n => n.RefundLottery(lotteryId, userOrg), default);
        }

        [Test]
        public void UpdateRefundFailedFlag_LotteryFound_FlagUpdated()
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            _sut.UpdateRefundFailedFlag(1, true, userOrg);

            _unitOfWork.Received().SaveChanges(userOrg.UserId);
        }

        [Test]
        public async Task FinishLotteryAsync_LotteryFound_ChangesStatus()
        {
            MockLotteries();
            var userOrg = GetUserOrg();

            await _sut.FinishLotteryAsync(1, userOrg);

            await _unitOfWork.Received().SaveChangesAsync(userOrg.UserId);
        }

        [TestCase(1)]
        public void GetLotteryStats_LotteryFound_ReturnsCorrectDTO(int lotteryId)
        {
            MockLotteries();
            _participantService.GetParticipantsCounted(lotteryId).Returns(GetParticipantDTO());
            var expected = new LotteryStatsDTO
            {
                TotalParticipants = 4,
                KudosSpent = 10,
                TicketsSold = 10
            };

            var result = _sut.GetLotteryStats(lotteryId, GetUserOrg());

            Assert.AreEqual(expected.TotalParticipants, result.TotalParticipants);
            Assert.AreEqual(expected.KudosSpent, result.KudosSpent);
            Assert.AreEqual(expected.TicketsSold, result.TicketsSold);
        }

        [Test]
        public void GetLotteries_OrganizationWithLotteries_ReturnsCorrectAmountOfLotteries()
        {
            MockLotteries();

            var result = _sut.GetLotteries(new UserAndOrganizationDTO { OrganizationId = 12345 });

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GetFilteredLotteries_FilterSet_ReturnsCorrectAmountOfLotteries()
        {
            MockLotteries();

            var result = _sut.GetFilteredLotteries("foo", GetUserOrg());

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GetLotteryStatus_ExistingLottery_ReturnsCorrectStatusObject()
        {
            MockLotteries();
            var expected = new LotteryStatusDTO
            {
                LotteryStatus = (int)LotteryStatus.Started,
                RefundFailed = false
            };

            var result = _sut.GetLotteryStatus(1, GetUserOrg());

            Assert.AreEqual(expected.LotteryStatus, result.LotteryStatus);
            Assert.AreEqual(expected.RefundFailed, result.RefundFailed);
        }

        [Test]
        public void BuyLotteryTicketAsync_NonExistentUser_StopsExecuting()
        {
            _userService.GetApplicationUser(default).ReturnsNullForAnyArgs();

            _sut.BuyLotteryTicketAsync(default, default);

            _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync((string)default);
        }

        [Test]
        public void BuyLotteryTicketAsync_NonExistentLottery_StopsExecuting()
        {
            MockLotteries();
            _userService.GetApplicationUser(default).ReturnsForAnyArgs(new ApplicationUser());

            _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDTO { LotteryId = int.MaxValue }, GetUserOrg());

            _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync((string)default);
        }

        [TestCase(1)]
        public void BuyLotteryTicketAsync_NotEnoughKudos_ThrowsException(int lotteryId)
        {
            MockParticipants();
            MockLotteries();
            _mapper.Map<Lottery, LotteryDetailsDTO>(default).ReturnsForAnyArgs(new LotteryDetailsDTO { EntryFee = 1 });
            _userService.GetApplicationUser(default).ReturnsForAnyArgs(new ApplicationUser { RemainingKudos = 0 });

            var ex = Assert.ThrowsAsync<LotteryException>(async () => await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDTO { LotteryId = lotteryId, Tickets = 10 }, GetUserOrg()));

            Assert.AreEqual("User does not have enough kudos for the purchase.", ex.Message);
        }

        [TestCase(1)]
        public async Task BuyLotteryTicketAsync_AbleToBuyTickets_BuysSuccessfully(int lotteryId)
        {
            MockParticipants();
            MockLotteries();
            _mapper.Map<Lottery, LotteryDetailsDTO>(default).ReturnsForAnyArgs(new LotteryDetailsDTO { EntryFee = 1, EndDate = DateTime.UtcNow.AddDays(10) });
            _userService.GetApplicationUser(default).ReturnsForAnyArgs(new ApplicationUser { RemainingKudos = 100 });

            await _sut.BuyLotteryTicketAsync(new BuyLotteryTicketDTO { LotteryId = lotteryId, Tickets = 10 }, GetUserOrg());

            await _unitOfWork.ReceivedWithAnyArgs().SaveChangesAsync((string)default);
        }

        [Test]
        public void GetRunningLotteries_OrganizationHasLotteries_ReturnsLotteries()
        {
            MockLotteries();

            var result = _sut.GetRunningLotteries(GetUserOrg());

            Assert.AreEqual(2, result.Count());
        }

        private static IList<LotteryDTO> GetCreateLotteryDTOList()
        {
            return new List<LotteryDTO>
            {
                new LotteryDTO { Id = 1, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor", UserId = "5", EntryFee = -5 },
                new LotteryDTO { Id = 2, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", UserId = "5", EntryFee = 2 },
                new LotteryDTO { Id = 3, OrganizationId = 1, Status = (int)LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(4), Title = "Table", UserId = "5", EntryFee = 2 },
                new LotteryDTO { Id = 4, OrganizationId = 1, Status = (int)LotteryStatus.Started, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDTO { Id = 5, OrganizationId = 1, Status = (int)LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDTO { Id = 6, OrganizationId = 1, Status = (int)LotteryStatus.Ended, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = 5 },
                new LotteryDTO { Id = 7, OrganizationId = 1, Status = (int)LotteryStatus.Ended, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = -5 }
            };
        }

        private static IList<LotteryParticipantDTO> GetParticipantDTO()
        {
            return new List<LotteryParticipantDTO>
            {
                new LotteryParticipantDTO
                {
                    Tickets = 2
                },
                new LotteryParticipantDTO
                {
                    Tickets = 3
                },
                new LotteryParticipantDTO
                {
                    Tickets = 2
                },
                new LotteryParticipantDTO
                {
                    Tickets = 3
                }
            };
        }

        private void MockLotteries()
        {
            var data = GetLotteries();

            _lotteriesDb.SetDbSetData(data.AsQueryable());
        }

        private IList<Lottery> GetLotteries()
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
                    Status = (int)LotteryStatus.Ended,
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
                    Status = (int)LotteryStatus.Ended,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(2)
                },
                new Lottery
                {
                    Id = 7,
                    Status = (int)LotteryStatus.Ended,
                    OrganizationId = 12345,
                    Title = string.Empty,
                    EndDate = DateTime.UtcNow.AddDays(3)
                }
            };
        }

        private void MockParticipants()
        {
            var data = GetParticipants();

            _lotteryParticipantsDb.SetDbSetData(data.AsQueryable());
        }

        private IList<LotteryParticipant> GetParticipants()
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
                EndDate = DateTime.Now.AddDays(5),
                Title = "1000 kudos",
                EntryFee = 5,
                IsRefundFailed = false
            };
        }

        private static UserAndOrganizationDTO GetUserOrg()
        {
            return new UserAndOrganizationDTO
            {
                UserId = "5",
                OrganizationId = 1
            };
        }
    }
}
