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
using Shrooms.DataTransferObjects.Models;
using Shrooms.Infrastructure.FireAndForget;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryServiceTests
    {
        private ILotteryService _lotteryService;


        private IUnitOfWork2 _unitOfWork;
        private IDbSet<Lottery> _lotteriesDbSet;

        private IParticipantService _participantSercice;
        private IUserService _userService;
        private IKudosService _kudosService;


        [SetUp]
        public void TestInitializers()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            
            _lotteriesDbSet = _unitOfWork.MockDbSet<Lottery>(GetLotteries().AsEnumerable());

            _unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDbSet);

            _participantSercice = Substitute.For<IParticipantService>();
            _userService = Substitute.For<IUserService>();
            _kudosService = Substitute.For<IKudosService>();

            var mapper = ModelMapper.Create();

            IAsyncRunner asyncRunner = Substitute.For<IAsyncRunner>();

            _lotteryService = new LotteryService(_unitOfWork, mapper, _participantSercice, _userService, _kudosService, asyncRunner);
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

        private List<Lottery> GetLotteries()
        {
            return new List<Lottery>
            {
                new Lottery() { Id = 1, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor",  EntryFee = -5 },
                new Lottery() { Id = 2, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", EntryFee = 2 },
                new Lottery() { Id = 3, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
                new Lottery() { Id = 4, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
                new Lottery() { Id = 5, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
                new Lottery() { Id = 6, OrganizationId = 1, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
                new Lottery() { Id = 7, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
                new Lottery() { Id = 8, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 },
            };
        }

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

        private List<EditDraftedLotteryDTO> GetEditDraftedLotteryDTOList()
        {
            return new List<EditDraftedLotteryDTO>
            {
                new EditDraftedLotteryDTO() { Id = 1, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(2),  Title = "Monitor", UserId = "5", EntryFee = 5 },
                new EditDraftedLotteryDTO() { Id = 2, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(-5),  Title = "Computer", UserId = "5", EntryFee = 2 },
                new EditDraftedLotteryDTO() { Id = 3, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(4), Title = "Table", UserId = "5", EntryFee = 2 },
                new EditDraftedLotteryDTO() { Id = 4, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", UserId = "5", EntryFee = 5 },
                new EditDraftedLotteryDTO() { Id = 5, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", UserId = "5", EntryFee = 5 },
                new EditDraftedLotteryDTO() { Id = 6, OrganizationId = 1, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", UserId = "5", EntryFee = 5 },
                new EditDraftedLotteryDTO() { Id = 7, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
                new EditDraftedLotteryDTO() { Id = 8, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 },

            };
        }

        private List<EditStartedLotteryDTO> GetEditStartedLotteryDTOList()
        {
            return new List<EditStartedLotteryDTO>
            {
                new EditStartedLotteryDTO() { Id = 3, Description = "updating", OrganizationId = 1},
                new EditStartedLotteryDTO() { Id = 5, Description = "updating", OrganizationId = 1},
                new EditStartedLotteryDTO() { Id = 6, Description = "updating", OrganizationId = 1},
                new EditStartedLotteryDTO() { Id = 1, Description = "new description", OrganizationId = 1},
                new EditStartedLotteryDTO() { Id = 2, Description = "changed", OrganizationId = 1},
                new EditStartedLotteryDTO() { Id = 4, Description = "updated", OrganizationId = 1},

            };

        }
    }
}
