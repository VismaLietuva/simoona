using NSubstitute;
using NUnit.Framework;
using Shrooms.Domain.Services.BlacklistStates;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.ModelMappings;
using Shrooms.Tests.Extensions;
using System.Threading.Tasks;
using Shrooms.Presentation.WebViewModels.Models.BlacklistStates;
using System.Threading;
using System.Net;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.DataTransferObjects;
using NSubstitute.ExceptionExtensions;
using Shrooms.Contracts.Exceptions;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class BlacklistStateControllerTests
    {
        private BlacklistStateController _blacklistStateController;

        private IBlacklistStateService _blacklistStateService;

        [SetUp]
        public void TestInitializer()
        {
            _blacklistStateService = Substitute.For<IBlacklistStateService>();

            _blacklistStateController = new BlacklistStateController(ModelMapper.Create(), _blacklistStateService);
            _blacklistStateController.SetUpControllerForTesting();
        }

        [Test]
        public async Task CreateBlacklistState_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new CreateBlacklistStateViewModel();

            // Act
            var httpActionResult = await _blacklistStateController.CreateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task CreateBlacklistState_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var args = new CreateBlacklistStateViewModel();

            _blacklistStateController.Validate(args);

            // Act
            var httpActionResult = await _blacklistStateController.CreateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task CreateBlacklistState_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            var args = new CreateBlacklistStateViewModel();

            _blacklistStateService
                .CreateAsync(Arg.Any<CreateBlacklistStateDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistStateController.CreateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklistState_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new UpdateBlacklistStateViewModel();

            // Act
            var httpActionResult = await _blacklistStateController.UpdateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklistState_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var args = new UpdateBlacklistStateViewModel();

            _blacklistStateController.Validate(args);

            // Act
            var httpActionResult = await _blacklistStateController.UpdateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task UpdateBlacklistState_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            var args = new UpdateBlacklistStateViewModel();

            _blacklistStateService
                .UpdateAsync(Arg.Any<UpdateBlacklistStateDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistStateController.UpdateBlacklistState(args);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task DeleteBlacklistState_ValidValues_ReturnsOk()
        {
            // Arrange
            const string userId = "";

            // Act
            var httpActionResult = await _blacklistStateController.DeleteBlacklistState(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task DeleteBlacklistState_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            const string userId = "";

            _blacklistStateService
                .DeleteAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Act
            var httpActionResult = await _blacklistStateController.DeleteBlacklistState(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetBlacklistState_ValidValues_ReturnsOk()
        {
            // Arrange
            const string userId = "";

            // Act
            var httpActionResult = await _blacklistStateController.DeleteBlacklistState(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetBlacklistState_WhenBlacklistStateIsNotFound_ReturnsNotFound()
        {
            // Arrange
            const string userId = "";

            _blacklistStateService
                .FindAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Returns((BlacklistStateDto)null);

            // Act
            var httpActionResult = await _blacklistStateController.GetBlacklistState(userId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
