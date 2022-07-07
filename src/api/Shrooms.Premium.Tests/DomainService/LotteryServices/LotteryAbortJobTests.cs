using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Premium.Domain.Services.Lotteries;

namespace Shrooms.Premium.Tests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryAbortJobTests
    {
        private ILotteryService _lotteryService;
        private ILotteryAbortJob _sut;
        private IKudosService _kudosService;
        private IParticipantService _participantService;
        private IUnitOfWork2 _unitOfWork;
        private IAsyncRunner _asyncRunner;

        [SetUp]
        public void SetUp()
        {
            _lotteryService = Substitute.For<ILotteryService>();

            _unitOfWork = Substitute.For<IUnitOfWork2>();

            _kudosService = Substitute.For<IKudosService>();
            _participantService = Substitute.For<IParticipantService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();
            var logger = Substitute.For<ILogger>();

            _sut = new LotteryAbortJob(_kudosService, _participantService, logger, _asyncRunner, _unitOfWork, _lotteryService);
        }

        [Test]
        public async Task RefundLottery_WrongLotteryId_Exits()
        {
            _lotteryService.GetLotteryAsync(1).ReturnsNull();

            await _sut.RefundLotteryAsync(1, _userAndOrganization);

            await _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCountedAsync(default);
        }

        [Test]
        public async Task RefundLottery_OrganizationIdDoesNotMatch_Exits()
        {
            var userOrg = new UserAndOrganizationDto { OrganizationId = 100 };
            _lotteryService.GetLotteryAsync(Arg.Any<int>()).ReturnsForAnyArgs(GetLottery());

            await _sut.RefundLotteryAsync(default, userOrg);

            await _participantService.DidNotReceiveWithAnyArgs().GetParticipantsCountedAsync(default);
        }

        [TestCase(LotteryStatus.Refunded)]
        [TestCase(LotteryStatus.Deleted)]
        [TestCase(LotteryStatus.Drafted)]
        [TestCase(LotteryStatus.Finished)]
        [TestCase(LotteryStatus.Started)]
        public async Task RefundLottery_IncorrectLotteryStatuses_DoesNotAddKudos(LotteryStatus status)
        {
            _lotteryService.GetLotteryAsync(Arg.Any<int>()).ReturnsForAnyArgs(new Lottery
            {
                Id = 1,
                OrganizationId = 1,
                Status = (int)status
            });

            await _sut.RefundLotteryAsync(default, _userAndOrganization);

            await _kudosService.DidNotReceiveWithAnyArgs().AddRefundKudosLogsAsync(default);
            await _kudosService.DidNotReceiveWithAnyArgs().UpdateProfilesFromUserIdsAsync(default, default);
        }

        [Test]
        public async Task RefundLottery_RefundableLottery_RefundsSuccessfully()
        {
            _lotteryService.GetLotteryAsync(Arg.Any<int>()).ReturnsForAnyArgs(new Lottery
                {
                    Id = 1,
                    OrganizationId = 1,
                    Status = (int)LotteryStatus.RefundStarted
                });

            await _sut.RefundLotteryAsync(1, _userAndOrganization);

            await _kudosService.ReceivedWithAnyArgs().UpdateProfilesFromUserIdsAsync(default, default);
            await _unitOfWork.ReceivedWithAnyArgs(requiredNumberOfCalls: 2).SaveChangesAsync((string)default);
        }

        [Test]
        public async Task RefundLottery_FailsGetKudosType_CatchesException()
        {
            _lotteryService.GetLotteryAsync(Arg.Any<int>()).ReturnsForAnyArgs(GetLottery());
            _kudosService
                .When(x => x.GetKudosTypeIdAsync(KudosTypeEnum.Refund))
                .Do(_ => throw new ArgumentNullException());

            await _sut.RefundLotteryAsync(default, _userAndOrganization);

            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryService>(default, default);
        }

        [Test]
        public async Task RefundLottery_FailsSaveChangesToDatabase_CatchesException()
        {
            var lottery = GetLottery();
            _lotteryService.GetLotteryAsync(Arg.Any<int>()).ReturnsForAnyArgs(lottery);

            _unitOfWork
                .When(async x => await x.SaveChangesAsync(_userAndOrganization.UserId))
                .Do(_ => throw new Exception());

            await _sut.RefundLotteryAsync(1, _userAndOrganization);

            _asyncRunner.ReceivedWithAnyArgs().Run<ILotteryService>(default, default);
        }

        private static Lottery GetLottery()
        {
            return new Lottery
            {
                Id = 1,
                OrganizationId = 1,
                Status = (int)LotteryStatus.RefundStarted,
                EndDate = DateTime.Now.AddDays(value: 2),
                Title = "Monitor",
                EntryFee = -5
            };
        }

        private readonly UserAndOrganizationDto _userAndOrganization = new UserAndOrganizationDto
        {
            OrganizationId = 1,
            UserId = "1"
        };
    }
}
