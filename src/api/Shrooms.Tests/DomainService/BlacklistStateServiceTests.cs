using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.BlacklistStates;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;
using System;
using System.Threading.Tasks;
using Shrooms.Tests.Extensions;
using System.Collections.Generic;
using System.Data.Entity;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class BlacklistStateServiceTests
    {
        private ISystemClock _systemClock;
        private IBlacklistStateValidator _validator;
        
        private DbSet<BlacklistState> _blacklistStateDbSet;

        private BlacklistStateService _blacklistStateService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _blacklistStateDbSet = uow.MockDbSetForAsync(new List<BlacklistState>());

            uow.GetDbSet<BlacklistState>()
                .Returns(_blacklistStateDbSet);

            _systemClock = Substitute.For<ISystemClock>();
            _systemClock.UtcNow.Returns(DateTime.UtcNow);
            
            _validator = Substitute.For<IBlacklistStateValidator>();

            _blacklistStateService = new BlacklistStateService(uow, _validator, _systemClock);
        }

        [Test]
        public void CreateAsync_WhenUserIsAlreadyBlacklisted_ThrowsValidationException()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistStateDto();

            _validator
                .CheckIfUserIsAlreadyBlacklistedAsync(Arg.Any<BlacklistStateDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.CreateAsync(args, userOrg));
        }

        [Test]
        public void CreateAsync_WhenUserDoesNotExist_ThrowsValidationException()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistStateDto();

            _validator
                .CheckIfUserExistsAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.CreateAsync(args, userOrg));
        }

        [Test]
        public async Task CreateAsync_WithValidValues_CreatesBlacklistState()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistStateDto
            {
                UserId = "Id",
                EndDate = DateTime.UtcNow,
                Reason = "Reason"
            };

            // Act
            var result = await _blacklistStateService.CreateAsync(args, userOrg);

            Assert.AreEqual(args.UserId, result.UserId);
        }

        [Test]
        public void DeleteAsync_BlacklistStateDoesNotExists_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";
            var blacklistStateToDelete = new BlacklistState
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(new List<BlacklistState> { blacklistStateToDelete });
            _validator
                .When(validator => validator.CheckIfBlacklistStateExists(Arg.Any<BlacklistState>()))
                .Do(validator => { throw new ValidationException(0); });

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.DeleteAsync(userId, userOrg));
        }

        [Test]
        public void DeleteAsync_WhenAllBlacklistStatesAreExpired_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";
            var blacklistStateToDelete = new BlacklistState
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(-10),
                OrganizationId = 1
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(new List<BlacklistState> { blacklistStateToDelete });
            _validator
                .When(validator => validator.CheckIfBlacklistStateExists(Arg.Is((BlacklistState)null)))
                .Do(validator => { throw new ValidationException(0); });

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.DeleteAsync(userId, userOrg));
        }

        [Test]
        public async Task DeleteAsync_WithValidValues_DeletesBlacklistState()
        {
            // Arrange
            var userId = "Id";
            var blacklistStateToDelete = new BlacklistState
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(new List<BlacklistState> { blacklistStateToDelete });

            // Act
            var result = await _blacklistStateService.DeleteAsync(userId, userOrg);

            // Assert
            Assert.AreEqual(blacklistStateToDelete.UserId, result.UserId);
        }

        [Test]
        public async Task FindAsync_WhenMoreThanOneBlacklistStateIsPresent_FindsActiveBlacklistState()
        {
            // Arrange
            var userId = "Id";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var shouldFindThis = new BlacklistState
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                shouldFindThis,
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Act
            var result = await _blacklistStateService.FindAsync(userId, userOrg);

            // Assert
            Assert.AreEqual(shouldFindThis.UserId, result.UserId);
        }

        [Test]
        public async Task FindAsync_WhenAllBlacklistStatesAreExpired_ReturnsNull()
        {
            // Arrange
            var userId = "Id";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var shouldFindThis = new BlacklistState
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(-10),
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                shouldFindThis,
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Act
            var result = await _blacklistStateService.FindAsync(userId, userOrg);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task FindAsync_WhenNoBlacklistStatesArePresent_ReturnsNull()
        {
            // Arrange
            var userId = "Id";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var shouldFindThis = new BlacklistState
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                shouldFindThis,
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Act
            var result = await _blacklistStateService.FindAsync(userId, userOrg);

            // Assert
            Assert.AreEqual(shouldFindThis.UserId, result.UserId);
        }

        [Test]
        public void UpdateAsync_WhenAllBlacklistStatesAreExpired_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";

            var updateDto = new UpdateBlacklistStateDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10),
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            _validator
               .When(validator => validator.CheckIfBlacklistStateExists(Arg.Is((BlacklistState)null)))
               .Do(validator => { throw new ValidationException(0); });

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.UpdateAsync(updateDto, userOrg));
        }

        [Test]
        public async Task UpdateAsync_WhenBlacklistStateIsNotFound_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";

            var updateDto = new UpdateBlacklistStateDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10),
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
            };

            _validator
               .When(validator => validator.CheckIfBlacklistStateExists(Arg.Is((BlacklistState)null)))
               .Do(validator => { throw new ValidationException(0); });

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistStateService.UpdateAsync(updateDto, userOrg));
        }

        [Test]
        public async Task UpdateAsync_WithValidValues_UpdatesBlacklistState()
        {
            // Arrange
            var userId = "Id";

            var updateDto = new UpdateBlacklistStateDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10),
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            _blacklistStateDbSet.SetDbSetDataForAsync(blacklistStates);

            // Act
            var result = await _blacklistStateService.UpdateAsync(updateDto, userOrg);

            // Assert
            Assert.AreEqual(updateDto.UserId, result.UserId);
            Assert.AreEqual(updateDto.Reason, result.Reason);
            Assert.AreEqual(updateDto.EndDate, result.EndDate);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenMoreThanOneBlacklistStateIsPresent_FindsActiveBlacklistState()
        {
            // Arrange
            var shouldFindThis = new BlacklistState
            {
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                shouldFindThis,
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            // Act 
            _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out var result);

            // Assert
            Assert.AreEqual(shouldFindThis.UserId, result.UserId);
            Assert.AreEqual(shouldFindThis.Reason, result.Reason);
            Assert.AreEqual(shouldFindThis.EndDate, result.EndDate);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenMoreThanOneBlacklistStateIsPresent_ReturnsTrue()
        {
            // Arrange
            var shouldFindThis = new BlacklistState
            {
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var blacklistStates = new List<BlacklistState>
            {
                shouldFindThis,
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1
                }
            };

            // Act 
            var result = _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out _);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenBlacklistStateIsNotFound_ReturnsFalse()
        {
            // Arrange
            var blacklistStates = new List<BlacklistState>();

            // Act 
            var result = _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out _);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenBlacklistStateIsNotFound_SetsResultToNull()
        {
            // Arrange
            var blacklistStates = new List<BlacklistState>();

            // Act 
            _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out var result);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenAllBlacklistStatesAreExpired_ReturnsFalse()
        {
            // Arrange
            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-20),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddYears(-20),
                    OrganizationId = 1
                }
            };

            // Act 
            var result = _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out _);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TryFindActiveBlacklistState_WhenAllBlacklistStatesAreExpired_SetsResultToNull()
        {
            // Arrange
            var blacklistStates = new List<BlacklistState>
            {
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-20),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1
                },
                new BlacklistState
                {
                    EndDate = DateTime.UtcNow.AddYears(-20),
                    OrganizationId = 1
                }
            };

            // Act 
            _blacklistStateService.TryFindActiveBlacklistState(blacklistStates, out var result);

            // Assert
            Assert.IsNull(result);
        }
    }
}
