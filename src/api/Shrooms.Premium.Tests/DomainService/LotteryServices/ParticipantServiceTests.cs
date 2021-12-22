using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.LotteryServices
{
    [TestFixture]
    public class ParticipantServiceTests
    {
        private IParticipantService _participantService;

        private IUnitOfWork2 _unitOfWork;
        private DbSet<LotteryParticipant> _lotteryParticipants;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteryParticipants = _unitOfWork.MockDbSetForAsync<LotteryParticipant>();

            _participantService = new ParticipantService(_unitOfWork);
        }

        [TestCase(5, 2)]
        [TestCase(1000, 0)]
        public async Task GetParticipantsCounted_AnyLotteryId_ReturnsCorrectTicketsCount(int lotteryId, int tickets)
        {
            _lotteryParticipants.SetDbSetDataForAsync(GetParticipants());

            var result = await _participantService.GetParticipantsCountedAsync(lotteryId);

            Assert.That(result, Is.All.Matches<LotteryParticipantDto>(x => x.Tickets == tickets));
        }

        [TestCase(1, 1, 2)]
        [TestCase(1, 1, 3)]
        public async Task GetPagedParticipants_Returns_Requested_Amount_Of_Participants(int lotteryId, int page, int pageSize)
        {
            _lotteryParticipants.SetDbSetDataForAsync(GetParticipants());

            var result = await _participantService.GetPagedParticipantsAsync(lotteryId, page, pageSize);

            Assert.IsTrue(result.Count == pageSize);
        }

        private IEnumerable<LotteryParticipant> GetParticipants()
        {
            var user1 = new ApplicationUser { Id = "1", FirstName = "Paul", LastName = "Giraffe" };
            var user2 = new ApplicationUser { Id = "2", FirstName = "Tom", LastName = "Bear" };
            var user3 = new ApplicationUser { Id = "3", FirstName = "John", LastName = "Zebra" };

            return new List<LotteryParticipant>
            {
                new LotteryParticipant { UserId = "1", LotteryId = 1, User = user1 },
                new LotteryParticipant { UserId = "1", LotteryId = 1, User = user1 },
                new LotteryParticipant { UserId = "1", LotteryId = 1, User = user1 },
                new LotteryParticipant { UserId = "1", LotteryId = 1, User = user1 },
                new LotteryParticipant { UserId = "1", LotteryId = 5, User = user1 },
                new LotteryParticipant { UserId = "1", LotteryId = 5, User = user1 },
                new LotteryParticipant { UserId = "2", LotteryId = 1, User = user2 },
                new LotteryParticipant { UserId = "2", LotteryId = 5, User = user2 },
                new LotteryParticipant { UserId = "2", LotteryId = 5, User = user2 },
                new LotteryParticipant { UserId = "3", LotteryId = 1, User = user3 },
                new LotteryParticipant { UserId = "3", LotteryId = 2, User = user3 },
                new LotteryParticipant { UserId = "3", LotteryId = 3, User = user3 },
                new LotteryParticipant { UserId = "3", LotteryId = 5, User = user3 },
                new LotteryParticipant { UserId = "3", LotteryId = 5, User = user3 }
            };
        }
    }
}
