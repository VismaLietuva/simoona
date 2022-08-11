using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Tests.Extensions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using System.Data.Entity;
using Shrooms.Premium.Domain.DomainServiceValidators.Lotteries;
using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Tests.DomainService.Validators
{
    [TestFixture]
    public class LotteryValidatorTests
    {
        private ISystemClock _systemClock;
        private DbSet<ApplicationUser> _usersDbSet;

        private LotteryValidator _sut;

        [SetUp]
        public void TestInitializer()
        {
            _systemClock = Substitute.For<ISystemClock>();

            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _sut = new LotteryValidator(_systemClock, uow);
        }

        [Test]
        public void CheckIfGiftReceiversExistAsync_AllReceiversExist_DoesNotThrow()
        {
            // Arrange
            var receiverId = Guid.NewGuid().ToString();
            var receivers = new List<string> { receiverId };

            var user = new ApplicationUser
            {
                Id = receiverId,
                OrganizationId = TestConstants.DefaultOrganizationId,
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "not related id",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });

            // Assert
            Assert.DoesNotThrowAsync(async () => await _sut.CheckIfGiftReceiversExistAsync(receivers, userOrg));
        }

        [Test]
        public void CheckIfGiftReceiversExistAsync_ReceiverIsNotFound_ThrowsLotteryException()
        {
            // Arrange
            var receiverId = "id";
            var receivers = new List<string> { receiverId };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                OrganizationId = TestConstants.DefaultOrganizationId,
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "not related id",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });

            // Assert
            Assert.ThrowsAsync<LotteryException>(async () => await _sut.CheckIfGiftReceiversExistAsync(receivers, userOrg));
        }

        [Test]
        public void CheckIfGiftReceiversExistAsync_ReceiverIsRequestMaker_ThrowsLotteryException()
        {
            // Arrange
            var receiverId = Guid.NewGuid().ToString();
            var receivers = new List<string> { receiverId };

            var user = new ApplicationUser
            {
                Id = receiverId,
                OrganizationId = TestConstants.DefaultOrganizationId,
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = receiverId,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });

            // Assert
            Assert.ThrowsAsync<LotteryException>(async () => await _sut.CheckIfGiftReceiversExistAsync(receivers, userOrg));
        }

        [Test]
        public void CheckIfGiftedTicketLimitIsExceeded_TotalTicketCountLessThanLimit_DoesNotThrow()
        {
            // Arrange
            var buyer = new LotteryDetailsBuyerDto
            {
                RemainingGiftedTicketCount = 10
            };

            var totalTicketCount = 3;

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfGiftedTicketLimitIsExceeded(buyer, totalTicketCount));
        }

        [Test]
        public void CheckIfGiftedTicketLimitIsExceeded_RemainingCountIsZero_ThrowsLotteryException()
        {
            // Arrange
            var buyer = new LotteryDetailsBuyerDto
            {
                RemainingGiftedTicketCount = 0
            };

            var totalTicketCount = 3;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfGiftedTicketLimitIsExceeded(buyer, totalTicketCount));
        }


        [Test]
        public void CheckIfGiftedTicketLimitIsExceeded_RemainingCountGreaterThanZeroAndTotalTicketCountIsGreaterThanLimit_ThrowsLotteryException()
        {
            // Arrange
            var buyer = new LotteryDetailsBuyerDto
            {
                RemainingGiftedTicketCount = 10
            };

            var totalTicketCount = 20;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfGiftedTicketLimitIsExceeded(buyer, totalTicketCount));
        }

        [Test]
        public void CheckIfBuyerExists_IsNull_ThrowsLotteryException()
        {
            // Arrange
            ApplicationUser buyer = null;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfBuyerExists(buyer));
        }

        [Test]
        public void CheckIfBuyerExists_IsNotNull_DoesNotThrow()
        {
            // Arrange
            var buyer = new ApplicationUser();

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfBuyerExists(buyer));
        }

        [Test]
        public void CheckIfLotteryExists_LotteryIsNull_ThrowsLotteryException()
        {
            // Arrange
            Lottery lottery = null;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryExists(lottery));
        }

        [Test]
        public void CheckIfLotteryExists_LotteryIsNotNull_DoesNotThrow()
        {
            // Arrange
            var lottery = new Lottery();

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryExists(lottery));
        }

        [Test]
        public void CheckIfLotteryExists_LotteryDetailsDtoIsNull_ThrowsLotteryException()
        {
            // Arrange
            LotteryDetailsDto lottery = null;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryExists(lottery));
        }

        [Test]
        public void CheckIfLotteryExists_LotteryDetailsDtoIsNotNull_DoesNotThrow()
        {
            // Arrange
            var lottery = new LotteryDetailsDto();

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryExists(lottery));
        }

        [Test]
        public void CheckIfLotteryEnded_EndDateLessThanCurrentDate_ThrowsLotteryException()
        {
            // Arrange
            var lottery = new LotteryDto
            {
                EndDate = DateTime.UtcNow.AddYears(-1)
            };

            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryEnded(lottery));
        }

        [Test]
        public void CheckIfLotteryEnded_EndDateGreaterThanCurrentDate_DoesNotThrow()
        {
            // Arrange
            var lottery = new LotteryDto
            {
                EndDate = DateTime.UtcNow.AddYears(2)
            };

            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryEnded(lottery));
        }

        [Test]
        public void CheckIfLotteryAllowsGifting_LimitIsLessOrEqualZero_ThrowsLotteryException()
        {
            // Arrange
            var lottery = new LotteryDetailsDto
            {
                GiftedTicketLimit = 0
            };

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryAllowsGifting(lottery));
        }

        [Test]
        public void CheckIfLotteryAllowsGifting_LimitIsGreaterThanZero_ThrowsLotteryException()
        {
            // Arrange
            var lottery = new LotteryDetailsDto
            {
                GiftedTicketLimit = 10
            };

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryAllowsGifting(lottery));
        }

        [Test]
        public void CheckIfUserHasEnoughKudos_TicketCostGreaterThanRemainingKudos_ThrowsLotteryException()
        {
            // Arrange
            var buyer = new ApplicationUser
            {
                RemainingKudos = 10
            };

            const int totalCost = 100;

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfUserHasEnoughKudos(buyer, totalCost));
        }

        [Test]
        public void CheckIfUserHasEnoughKudos_TicketCostLessOrEqualToRemainingKudos_DoesNotThrow()
        {
            // Arrange
            var buyer = new ApplicationUser
            {
                RemainingKudos = 100
            };

            const int totalCost = 100;

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfUserHasEnoughKudos(buyer, totalCost));
        }

        [Test]
        public void CheckIfLotteryIsDrafted_LotteryIsNotDrafted_ThrowsLotteryException()
        {
            // Arrange
            var lottery = new Lottery
            {
                Status = LotteryStatus.Started
            };

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryIsDrafted(lottery));
        }

        [Test]
        public void CheckIfLotteryIsDrafted_LotteryIsDrafted_DoesNotThrow()
        {
            // Arrange
            var lottery = new Lottery
            {
                Status = LotteryStatus.Drafted
            };

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryIsDrafted(lottery));
        }

        [Test]
        public void CheckIfLotteryIsStarted_LotteryIsNotStarted_ThrowsLotteryException()
        {
            // Arrange
            var lottery = new Lottery
            {
                Status = LotteryStatus.Deleted
            };

            // Assert
            Assert.Throws<LotteryException>(() => _sut.CheckIfLotteryIsStarted(lottery));
        }

        [Test]
        public void CheckIfLotteryIsStarted_LotteryIsStarted_DoesNotThrow()
        {
            // Arrange
            var lottery = new Lottery
            {
                Status = LotteryStatus.Started
            };

            // Assert
            Assert.DoesNotThrow(() => _sut.CheckIfLotteryIsStarted(lottery));
        }


        [Test]
        public void IsValidTicketCount_CountLessOrEqualZero_ReturnsFalse()
        {
            // Arrange
            var buyDto = new BuyLotteryTicketsDto
            {
                TicketCount = -10
            };

            // Act
            var actual = _sut.IsValidTicketCount(buyDto);

            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void IsValidTicketCount_CountGreaterThanZero_ReturnsTrue()
        {
            // Arrange
            var buyDto = new BuyLotteryTicketsDto
            {
                TicketCount = 1
            };

            // Act
            var actual = _sut.IsValidTicketCount(buyDto);

            // Assert
            Assert.IsTrue(actual);
        }
    }
}
