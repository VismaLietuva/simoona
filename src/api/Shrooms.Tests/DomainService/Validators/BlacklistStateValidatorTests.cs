using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using System;
using System.Data.Entity;
using Shrooms.Tests.Extensions;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;

namespace Shrooms.Tests.DomainService.Validators
{
    [TestFixture]
    public class BlacklistStateValidatorTests
    {
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<BlacklistState> _blacklistStatesDbSet;

        private BlacklistStateValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            var systemClock = Substitute.For<ISystemClock>();

            systemClock.UtcNow.Returns(DateTime.UtcNow);

            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _blacklistStatesDbSet = uow.MockDbSetForAsync<BlacklistState>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<BlacklistState>().Returns(_blacklistStatesDbSet);

            _validator = new BlacklistStateValidator(systemClock, uow);
        }

        [Test]
        public void CheckIfBlacklistStateExists_WhenBlacklistStateIsNull_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => _validator.CheckIfBlacklistStateExists(null));
        }

        [Test]
        public void CheckIfBlacklistStateExists_WhenBlacklistStateIsNotNull_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _validator.CheckIfBlacklistStateExists(new BlacklistState()));
        }

        [Test]
        public void CheckIfUserExistsAsync_WhenUserIsFound_DoesNotThrow()
        {
            // Arrange
            var userId = "Id";
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = userId,
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _usersDbSet.SetDbSetDataForAsync(users);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserExistsAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserExistsAsync_WhenUserIsNotFound_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "Other id",
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _usersDbSet.SetDbSetDataForAsync(users);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _validator.CheckIfUserExistsAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistStateIsFound_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";

            var blacklistStateDto = new BlacklistStateDto
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddYears(1)
            };

            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStatesDbSet.SetDbSetDataForAsync(blacklistStates);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(blacklistStateDto, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistStatesAreExpired_DoesNotThrow()
        {
            // Arrange
            var userId = "Id";

            var blacklistStateDto = new BlacklistStateDto
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddYears(1)
            };

            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddYears(-1),
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStatesDbSet.SetDbSetDataForAsync(blacklistStates);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(blacklistStateDto, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistStateIsNotFound_DoesNotThrow()
        {
            // Arrange
            var userId = "Id";

            var blacklistStateDto = new BlacklistStateDto
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddYears(1)
            };

            var blacklistStates = new List<BlacklistState>();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStatesDbSet.SetDbSetDataForAsync(blacklistStates);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(blacklistStateDto, userOrg));
        }
    }
}
