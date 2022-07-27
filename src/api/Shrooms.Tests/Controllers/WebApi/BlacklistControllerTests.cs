using NSubstitute;
using NUnit.Framework;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.ModelMappings;
using Shrooms.Tests.Extensions;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Shrooms.Contracts.DataTransferObjects;
using NSubstitute.ExceptionExtensions;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using System.Collections.Generic;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class BlacklistControllerTests
    {
        private BlacklistController _blacklistController;

        private IBlacklistService _blacklistService;

        [SetUp]
        public void TestInitializer()
        {
            _blacklistService = Substitute.For<IBlacklistService>();

            _blacklistController = new BlacklistController(ModelMapper.Create(), _blacklistService);
            _blacklistController.SetUpControllerForTesting();
        }

        [Test]
        public async Task AddToBlacklist_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new CreateBlacklistUserViewModel();

            // Act
            var httpActionResult = await _blacklistController.AddToBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task AddToBlacklist_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var args = new CreateBlacklistUserViewModel();

            _blacklistController.Validate(args);

            // Act
            var httpActionResult = await _blacklistController.AddToBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task AddToBlacklist_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            var args = new CreateBlacklistUserViewModel();

            _blacklistService
                .CreateAsync(Arg.Any<CreateBlacklistUserDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistController.AddToBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklist_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new UpdateBlacklistUserViewModel();

            // Act
            var httpActionResult = await _blacklistController.UpdateBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklist_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var args = new UpdateBlacklistUserViewModel();

            _blacklistController.Validate(args);

            // Act
            var httpActionResult = await _blacklistController.UpdateBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklist_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            var args = new UpdateBlacklistUserViewModel();

            _blacklistService
                .UpdateAsync(Arg.Any<UpdateBlacklistUserDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistController.UpdateBlacklist(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task CancelBlacklist_ValidValues_ReturnsOk()
        {
            // Arrange
            const string userId = "";

            // Act
            var httpActionResult = await _blacklistController.CancelBlacklist(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task CancelBlacklist_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            const string userId = "";

            _blacklistService
                .CancelAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistController.CancelBlacklist(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetActiveBlacklist_ValidValues_ReturnsOk()
        {
            // Arrange
            _blacklistService.FindAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(new BlacklistUserDto());

            const string userId = "";

            // Act
            var httpActionResult = await _blacklistController.GetActiveBlacklist(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetActiveBlacklist_WhenBlacklistUserIsNotFound_ReturnsNotFound()
        {
            // Arrange
            const string userId = "";

            _blacklistService
                .FindAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Returns((BlacklistUserDto)null);

            // Act
            var httpActionResult = await _blacklistController.GetActiveBlacklist(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetBlacklistHistory_ValidValues_ReturnsOk()
        {
            // Arrange
            const string userId = "";

            _blacklistService
                .GetAllExceptActiveAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Returns(new List<BlacklistUserDto>());

            // Act
            var httpActionResult = await _blacklistController.GetBlacklistHistory(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetBlacklistHistory_WhenExceptionIsThrown_ReturnsForbidden()
        {
            // Arrange
            const string userId = "";

            _blacklistService
                .GetAllExceptActiveAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistController.GetBlacklistHistory(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
