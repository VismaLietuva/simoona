using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Tests.Extensions;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Tests.DomainService.LotteryServices
{
    [TestFixture]
    public class LotteryStatusChangeServiceTests
    {
        private ISystemClock _systemClock;
        private DbSet<Lottery> _lotteriesDbSet;
        private IUnitOfWork2 _uow;

        private ILotteryStatusChangeService _lotteryStatusChangeService;

        [SetUp]
        public void TestInitializer()
        {
            _systemClock = Substitute.For<ISystemClock>();
            _uow = Substitute.For<IUnitOfWork2>();
            _lotteriesDbSet = _uow.MockDbSetForAsync<Lottery>();

            _uow.GetDbSet<Lottery>().Returns(_lotteriesDbSet);

            _lotteryStatusChangeService = new LotteryStatusChangeService(_systemClock, _uow);
        }

        [Test]
        public async Task ProcessExpiredLotteries_NothingToProcess_DoesNothing()
        {
            // Arrange
            var lotteries = new List<Lottery>
            {
                new Lottery
                {
                    Status = (int)LotteryStatus.Expired,
                    EndDate = DateTime.UtcNow
                },

                new Lottery
                {
                    Status = (int)LotteryStatus.Expired,
                    EndDate = DateTime.UtcNow
                }
            };

            _lotteriesDbSet.SetDbSetDataForAsync(lotteries);

            // Act
            await _lotteryStatusChangeService.ProcessExpiredLotteriesAsync();

            // Assert
            await _uow.Received(0).SaveChangesAsync(Arg.Any<bool>());
        }

        [Test]
        public async Task ProcessExpiredLotteries_FindsLotteriesThatAreStartedAndEndDateHasPassed_ChangesStatus()
        {
            // Arrange
            _systemClock.UtcNow.Returns(DateTime.UtcNow.AddDays(1000));

            var lotteries = new List<Lottery>
            {
                new Lottery
                {
                    Status = (int)LotteryStatus.Started,
                    EndDate = DateTime.UtcNow
                },

                new Lottery
                {
                    Status = (int)LotteryStatus.Started,
                    EndDate = DateTime.UtcNow
                }
            };

            _lotteriesDbSet.SetDbSetDataForAsync(lotteries);

            // Act
            await _lotteryStatusChangeService.ProcessExpiredLotteriesAsync();

            // Assert
            await _uow.Received(1).SaveChangesAsync(Arg.Any<bool>());
        }
    }
}
