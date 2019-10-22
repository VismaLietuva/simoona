using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.Domain.Services.UserService;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Premium.UnitTests.ModelMappings;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _lotteryService;


        private IUnitOfWork2 _unitOfWork;
        private IDbSet<Lottery> _lotteriesDbSet;
        private IDbSet<LotteryParticipant> _participantsDbSet;

        private IParticipantService _participantSercice;
        private IUserService _userService;
        private IKudosService _kudosService;


        [SetUp]
        public void TestInitializers()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDbSet = _unitOfWork.MockDbSet<Lottery>();
            _participantsDbSet = _unitOfWork.MockDbSet<LotteryParticipant>();

            _participantSercice = Substitute.For<IParticipantService>();
            _userService = Substitute.For<IUserService>();
            _kudosService = Substitute.For<IKudosService>();

            var mapper = ModelMapper.Create();

            _lotteryService = new LotteryService(_unitOfWork, mapper, _participantSercice, _userService, _kudosService);
        }

        [TestCase]
        public void CreateLottery_Should_Throw_Exception_When_Given_Lotter_With_Negative_Entry_Fee()
        {
            var lottery = GetCreateLotteryDTOList()[0];

            Assert.ThrowsAsync<LotteryException>(async () => await _lotteryService.CreateLottery(lottery));
        }

        [TestCase]
        public void CreateLottery_Should_Throw_Exception_When_Given_Lotter_With_Past_End_Date()
        {
            var lottery = GetCreateLotteryDTOList()[1];

            Assert.ThrowsAsync<LotteryException>(async () => await _lotteryService.CreateLottery(lottery));
        }

        [TestCase]
        public void CreateLottery_Should_Throw_Exception_When_Given_Lotter_With_Status_Of_Aborted()
        {
            var lottery = GetCreateLotteryDTOList()[2];

            Assert.ThrowsAsync<LotteryException>(async () => await _lotteryService.CreateLottery(lottery));
        }

        //[TestCase(4)]
        //[TestCase(5)]
        //[TestCase(6)]
        //public void EditDraftedLottery_Should_Throw_Exception_When_Given_Lottery_With_Incorrect_Status()
        //{
        //    var nonDraftedLottery = GetCreateLotteryDTOList()
        //}

        private List<CreateLotteryDTO> GetCreateLotteryDTOList()
        {
            return new List<CreateLotteryDTO>
            {
                new CreateLotteryDTO() { Id = 1, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor", UserId = "5", EntryFee = -5 },
                new CreateLotteryDTO() { Id = 2, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", UserId = "5", EntryFee = 2 },
                new CreateLotteryDTO() { Id = 3, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", UserId = "5", EntryFee = 2 },
                new CreateLotteryDTO() { Id = 4, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", UserId = "5", EntryFee = 5 },
                new CreateLotteryDTO() { Id = 5, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", UserId = "5", EntryFee = 5 },
                new CreateLotteryDTO() { Id = 6, OrganizationId = 1, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = 5 },

            };
        }
    }
}
