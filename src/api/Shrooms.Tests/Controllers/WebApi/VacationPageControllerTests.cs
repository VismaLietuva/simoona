using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.Domain.Services.VacationPages;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models.VacationPage;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;
using System.Threading.Tasks;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class VacationPageControllerTests
    {
        private VacationPageController _vacationPageController;

        private IVacationPageService _vacationPageService;

        [SetUp]
        public void TestInitializer()
        {
            _vacationPageService = Substitute.For<IVacationPageService>();

            _vacationPageController = new VacationPageController(_vacationPageService, ModelMapper.Create());
            _vacationPageController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetVacationPage_Should_Return_Ok()
        {
            // Arrange
            _vacationPageService.GetVacationPage(Arg.Any<int>()).Returns(new VacationPageDto());

            // Act
            var result = await _vacationPageController.GetVacationPage();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetVacationPage_Should_Return_NotFound_If_VacationPage_Does_Not_Exist()
        {
            // Arrange
            _vacationPageService.GetVacationPage(Arg.Any<int>()).ReturnsNull();

            // Act
            var result = await _vacationPageController.GetVacationPage();

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task EditVacationPage_Should_Return_BadRequest_If_Invalid_ModelState()
        {
            // Arrange
            var vacationPageViewModel = new VacationPageViewModel();

            _vacationPageController.ModelState.AddModelError("key", "error");

            // Act
            var result = await _vacationPageController.EditVacationPage(vacationPageViewModel);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task EditVacationPage_Should_Return_Ok()
        {
            // Arrange
            _vacationPageService.EditVacationPage(Arg.Any<UserAndOrganizationDto>(), Arg.Any<VacationPageDto>())
                .Returns(Task.CompletedTask);

            var vacationPageViewModel = new VacationPageViewModel
            {
                Content = "test"
            };

            // Act
            var result = await _vacationPageController.EditVacationPage(vacationPageViewModel);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}