using System;
using System.Data.Entity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Kudos;
using Shrooms.EntityModels.Models.Lottery;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries;

namespace Shrooms.Premium.UnitTests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryAbortJobTests
    {
        private ILotteryAbortJob _sut;
        private IKudosService _kudosService;
        private IParticipantService _participantService;
        private IDbSet<Lottery> _lotteriesDb;
        private IUnitOfWork2 _unitOfWork;
        private IAsyncRunner _asyncRunner;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();
            _lotteriesDb = Substitute.For<IDbSet<Lottery>>();
            _unitOfWork.GetDbSet<Lottery>().Returns(_lotteriesDb);

            _kudosService = Substitute.For<IKudosService>();
            _participantService = Substitute.For<IParticipantService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();
            var logger = Substitute.For<ILogger>();

            _sut = new LotteryAbortJob(_kudosService, _participantService, logger, _asyncRunner, _unitOfWork);
        }

        [Test]
        public void RefundLottery_WrongLotteryId_Exits()
        {
            _lotteriesDb.Find().ReturnsNull();

            _sut.RefundLottery(default, GetUserOrg());

            _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCounted(default);
        }

        [Test]
        public void RefundLottery_OrganizationIdDoesNotMatch_Exits()
        {
            var userOrg = new UserAndOrganizationDTO { OrganizationId = 100 };
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());

            _sut.RefundLottery(default, userOrg);

            _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCounted(default);
        }

        [TestCase(BusinessLayerConstants.LotteryStatus.Refunded)]
        [TestCase(BusinessLayerConstants.LotteryStatus.Deleted)]
        [TestCase(BusinessLayerConstants.LotteryStatus.Drafted)]
        [TestCase(BusinessLayerConstants.LotteryStatus.Ended)]
        [TestCase(BusinessLayerConstants.LotteryStatus.Started)]
        public void RefundLottery_IncorrectLotteryStatuses_DoesNotAddKudos(BusinessLayerConstants.LotteryStatus status)
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(
                new Lottery
                {
                    Id = 1,
                    OrganizationId = 1,
                    Status = (int) status
                });

            _sut.RefundLottery(default, GetUserOrg());

            _kudosService.DidNotReceiveWithAnyArgs().AddRefundKudosLogs(default);
            _kudosService.DidNotReceiveWithAnyArgs().UpdateProfilesFromUserIds(default, default);
        }

        [Test]
        public void RefundLottery_RefundableLottery_RefundsSuccessfully()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(
                new Lottery
                {
                    Id = 1,
                    OrganizationId = 1,
                    Status = (int) BusinessLayerConstants.LotteryStatus.RefundStarted
                });

            _sut.RefundLottery(default, GetUserOrg());

            _kudosService.ReceivedWithAnyArgs().UpdateProfilesFromUserIds(default, default);
            _unitOfWork.ReceivedWithAnyArgs(requiredNumberOfCalls: 2).SaveChanges((string)default);
        }

        [Test]
        public void RefundLottery_FailsGetKudosType_CatchesException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());
            _kudosService
                .When(x => x.GetKudosTypeId(BusinessLayerConstants.KudosTypeEnum.Refund))
                .Do(x => throw new ArgumentNullException());

            _sut.RefundLottery(default, GetUserOrg());

            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryService>(default, default);
        }

        [Test]
        public void RefundLottery_FailsSaveChangesToDatabase_CatchesException()
        {
            _lotteriesDb.Find().ReturnsForAnyArgs(GetLottery());
            _unitOfWork
                .When(x => x.SaveChanges(GetUserOrg().UserId))
                .Do(x => throw new Exception());

            _sut.RefundLottery(default, GetUserOrg());

            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryService>(default, default);
        }

        private static Lottery GetLottery()
        {
            return new Lottery
            {
                Id = 1,
                OrganizationId = 1,
                Status = (int)BusinessLayerConstants.LotteryStatus.RefundStarted,
                EndDate = DateTime.Now.AddDays(value: 2),
                Title = "Monitor",
                EntryFee = -5
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
