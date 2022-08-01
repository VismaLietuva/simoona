using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers;
using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Tests.Extensions;
using Shrooms.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;
using System.Linq;

namespace Shrooms.Tests.DomainService.WebHookCallbacks
{
    [TestFixture]
    public class BlacklistUserStatusChangeWebHookServiceTests
    {
        private DbSet<BlacklistUser> _blacklistUsersDbSet;
        private ISystemClock _systemClock;
        private IUnitOfWork2 _uow;

        private BlacklistUserStatusChangeWebHookService _blacklistUserStatusChangeWebHookService;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _systemClock = Substitute.For<ISystemClock>();
            _blacklistUsersDbSet = _uow.MockDbSetForAsync<BlacklistUser>();

            _uow.GetDbSet<BlacklistUser>().Returns(_blacklistUsersDbSet);

            _blacklistUserStatusChangeWebHookService = new BlacklistUserStatusChangeWebHookService(_uow, _systemClock);
        }

        [Test]
        public async Task ProcessExpiredBlacklistUsersAsync_WhenActiveBlacklistEntriesAreExpired_ChangesBlacklistEntriesStatusToExpired()
        {
            // Arrange
            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            var blacklistUsers = new List<BlacklistUser>
            {
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(-3),
                    Status = BlacklistStatus.Active,
                },
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Status = BlacklistStatus.Active,
                },
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(-4),
                    Status = BlacklistStatus.Active,
                }
            };
            
            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            await _blacklistUserStatusChangeWebHookService.ProcessExpiredBlacklistUsersAsync();

            // Assert
            Assert.That(blacklistUsers.Where(entry => entry.Status == BlacklistStatus.Active), Is.All.Matches<BlacklistUser>(entry => entry.EndDate >= DateTime.UtcNow));
            Assert.That(blacklistUsers.Where(entry => entry.Status == BlacklistStatus.Expired), Is.All.Matches<BlacklistUser>(entry => entry.EndDate < DateTime.UtcNow));
        }

        [Test]
        public async Task ProcessExpiredBlacklistUsersAsync_WhenAllBlacklistEntriesAreWithCorrectStatus_DoesNothing()
        {
            // Arrange
            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            var blacklistUsers = new List<BlacklistUser>
            {
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(-3),
                    Status = BlacklistStatus.Expired,
                },
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Status = BlacklistStatus.Expired,
                },
                new BlacklistUser
                {
                    EndDate = DateTime.UtcNow.AddDays(-4),
                    Status = BlacklistStatus.Canceled,
                }
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            await _blacklistUserStatusChangeWebHookService.ProcessExpiredBlacklistUsersAsync();

            // Assert
            await _uow.Received(0).SaveChangesAsync(Arg.Any<bool>());
        }
    }
}
