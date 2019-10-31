using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Logger;
using Shrooms.UnitTests.Extensions;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryAbortJobTests
    {
        private ILotteryAbortJob _sut;

        private IKudosService _kudosService;

        private IParticipantService _participantService;

        private IDbSet<Lottery> _lotteriesDb;

        [SetUp]
        public void SetUp()
        {
            var unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDb = unitOfWork.MockDbSet(GetLotteries().AsEnumerable());
            unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDb);

            _kudosService = Substitute.For<IKudosService>();
            _participantService = Substitute.For<IParticipantService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            var logger = Substitute.For<ILogger>();

            _sut = new LotteryAbortJob(_kudosService, _participantService, logger, asyncRunner, unitOfWork);
        }

        [Test]
        public void RefundLottery_WrongLotteryId_Exits()
        {
            _lotteriesDb.Find(keyValues: default).ReturnsNull();

            _sut.RefundLottery(1, GetUserOrg());

            _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCounted(default);
        }

        [TestCase(1)]
        public void RefundLottery_OrganizationIdDoesNotMatch_Exits(int lotteryId)
        {
            var userOrg = new UserAndOrganizationDTO { OrganizationId = 100 };
            _lotteriesDb.Find(lotteryId).Returns(GetLotteries().First());

            _sut.RefundLottery(1, userOrg);

            _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCounted(default);
        }

        [TestCase(LotteryStatus.Refunded, 1)]
        [TestCase(LotteryStatus.Deleted, 1)]
        [TestCase(LotteryStatus.Drafted, 1)]
        [TestCase(LotteryStatus.Ended, 1)]
        public void RefundLottery_IncorrectLotteryStatuses_DoesNotAddKudos(LotteryStatus status, int lotteryId)
        {
            _lotteriesDb.Find(lotteryId).Returns(GetLotteries().First());

            _sut.RefundLottery(lotteryId, GetUserOrg());

            _kudosService.DidNotReceiveWithAnyArgs().AddRefundKudosLogs(default);
            _kudosService.DidNotReceiveWithAnyArgs().UpdateProfilesFromUserIds(default, default);
        }

        private static IEnumerable<Lottery> GetLotteries()
        {
            return new List<Lottery>
            {
                new Lottery { Id = 1, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(value: 2),  Title = "Monitor",  EntryFee = -5 },
                new Lottery { Id = 2, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(value: -5),  Title = "Computer", EntryFee = 2 },
                new Lottery { Id = 3, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(value: 4), Title = "Table", EntryFee = 2 },
                new Lottery { Id = 4, OrganizationId = 1, Status = 2, EndDate = DateTime.Now.AddDays(value: 5), Title = "1000 kudos", EntryFee = 5 },
                new Lottery { Id = 5, OrganizationId = 1, Status = 3, EndDate = DateTime.Now.AddDays(value: 5), Title = "100 kudos", EntryFee = 5 },
                new Lottery { Id = 6, OrganizationId = 1, Status = 4, EndDate = DateTime.Now.AddDays(value: 5), Title = "10 kudos", EntryFee = 5 },
                new Lottery { Id = 7, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(value: 5), Title = "10000 kudos", EntryFee = 5 },
                new Lottery { Id = 8, OrganizationId = 1, Status = 1, EndDate = DateTime.Now.AddDays(value: 5), Title = "10 000 kudos", EntryFee = 5 }
            };
        }

        private static UserAndOrganizationDTO GetUserOrg()
        {
            return new UserAndOrganizationDTO
            {
                UserId = "1",
                OrganizationId = 1
            };
        }
    }
}
