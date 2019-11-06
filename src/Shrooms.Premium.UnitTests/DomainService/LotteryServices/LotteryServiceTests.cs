using System;
using System.Collections.Generic;
using System.Data.Entity;
using AutoMapper;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.Domain.Services.UserService;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _sut;
        private IUnitOfWork2 _unitOfWork;
        private IDbSet<Lottery> _lotteriesDb;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDb = Substitute.For<IDbSet<Lottery>>();
            _unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDb);

            var asyncRunner = Substitute.For<IAsyncRunner>();
            var participantService = Substitute.For<IParticipantService>();
            var userService = Substitute.For<IUserService>();
            var kudosService = Substitute.For<IKudosService>();
            _mapper = Substitute.For<IMapper>();

            _sut = new LotteryService(_unitOfWork, _mapper, participantService, userService, kudosService, asyncRunner);
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
            _mapper.Map<CreateLotteryDTO, Lottery>(lotteryDTO).Returns(GetLottery());

            _sut.CreateLottery(lotteryDTO);

            _lotteriesDb.ReceivedWithAnyArgs().Add(default);
            _unitOfWork.Received().SaveChangesAsync(lotteryDTO.UserId);
        }

        [Test]
        public void EditDraftedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            var result = Assert.ThrowsAsync<LotteryException>(async () =>
                await _sut.EditDraftedLottery(new EditDraftedLotteryDTO()));

            Assert.That(result.Message, Is.EqualTo("Editing is forbidden for not drafted lottery."));
        }

        [Test]
        public void EditDraftedLottery_CorrectLotteryDTO_EditsLotterySuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery(LotteryStatus.Drafted));

            _sut.EditDraftedLottery(new EditDraftedLotteryDTO());

            _unitOfWork.Received().SaveChangesAsync(false);
        }

        [Test]
        public void EditStartedLottery_IncorrectLotteryStatus_ThrowsException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery(LotteryStatus.Refunded));

            var result = Assert.ThrowsAsync<LotteryException>(async () =>
                await _sut.EditStartedLottery(new EditStartedLotteryDTO()));

            Assert.That(result.Message, Is.EqualTo("Lottery is not running."));
        }

        [Test]
        public void EditStartedLottery_CorrectLotteryDTO_EditsLotterySuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            _sut.EditStartedLottery(new EditStartedLotteryDTO());

            _unitOfWork.Received().SaveChangesAsync(false);
        }

        [Test]
        public void GetLotteryDetails_NonExistentLottery_ReturnsNull()
        {
            _lotteriesDb.Find().ReturnsNullForAnyArgs();

            var result = _sut.GetLotteryDetails(default);

            Assert.IsNull(result);
        }

        [Test]
        public void AbortLottery_NonExistentLottery_ReturnsFalse()
        {
            _lotteriesDb.Find().ReturnsNullForAnyArgs();

            var result = _sut.AbortLottery(default, default);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void AbortLottery_StartedLottery_AbortedSuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            _sut.AbortLottery(4, GetUserOrg());
        }

        private static IList<CreateLotteryDTO> GetCreateLotteryDTOList()
        {
            return new List<CreateLotteryDTO>
            {
                new CreateLotteryDTO { Id = 1, OrganizationId = 1, Status = (int) LotteryStatus.Started, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor", UserId = "5", EntryFee = -5 },
                new CreateLotteryDTO { Id = 2, OrganizationId = 1, Status = (int) LotteryStatus.Started, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", UserId = "5", EntryFee = 2 },
                new CreateLotteryDTO { Id = 3, OrganizationId = 1, Status = (int) LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(4), Title = "Table", UserId = "5", EntryFee = 2 },
                new CreateLotteryDTO { Id = 4, OrganizationId = 1, Status = (int) LotteryStatus.Started, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", UserId = "5", EntryFee = 5 },
                new CreateLotteryDTO { Id = 5, OrganizationId = 1, Status = (int) LotteryStatus.Deleted, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", UserId = "5", EntryFee = 5 },
                new CreateLotteryDTO { Id = 6, OrganizationId = 1, Status = (int) LotteryStatus.Ended, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = 5 },
                new CreateLotteryDTO { Id = 7, OrganizationId = 1, Status = (int) LotteryStatus.Ended, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = -5 }
            };
        }

        private static Lottery GetLottery(LotteryStatus status = LotteryStatus.Started)
        {
            return  new Lottery
            {
                Id = 4,
                OrganizationId = 1,
                Status = (int)status,
                EndDate = DateTime.Now.AddDays(5),
                Title = "1000 kudos",
                EntryFee = 5
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
