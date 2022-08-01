using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.ExternalLinks;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models.ExternalLink;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class ExternalLinkControllerTests
    {
        private IExternalLinkService _externalLinkService;

        private ExternalLinkController _externalLinkController;

        [SetUp]
        public void TestInitializer()
        {
            _externalLinkService = Substitute.For<IExternalLinkService>();

            _externalLinkController = new ExternalLinkController(_externalLinkService, ModelMapper.Create());
            _externalLinkController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetAll_Should_Return_Ok()
        {
            // Act
            var httpActionResult = await _externalLinkController.GetAll();
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task UpdateLinks_Should_Return_Ok()
        {
            // Arrange
            var updateLinksViewModel = new ManageExternalLinkViewModel();

            // Act
            var httpActionResult = await _externalLinkController.UpdateLinks(updateLinksViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task UpdateLinks_Should_Return_BadRequest_If_ModelState_Is_Not_Valid()
        {
            // Arrange
            var updateLinksViewModel = new ManageExternalLinkViewModel
            {
                LinksToCreate = new List<NewExternalLinkViewModel>
                {
                    new NewExternalLinkViewModel
                    {
                        Priority = -10
                    }
                }
            };

            _externalLinkController.Validate(updateLinksViewModel);

            // Act
            var httpActionResult = await _externalLinkController.UpdateLinks(updateLinksViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Test]
        public async Task UpdateLinks_Should_Return_BadRequest_If_Update_Fails()
        {
            // Arrange
            _externalLinkService.UpdateLinksAsync(Arg.Any<ManageExternalLinkDto>())
                .Throws(new ValidationException(0));

            var updateLinksViewModel = new ManageExternalLinkViewModel();

            // Act
            var httpActionResult = await _externalLinkController.UpdateLinks(updateLinksViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
