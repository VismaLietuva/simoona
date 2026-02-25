using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.Employees;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models.Employees;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;
using System.Threading.Tasks;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class EmployeeControllerTests
    {
        private EmployeeController _employeeController;

        private IEmployeeListingService _employeeListingService;

        [SetUp]
        public void TestInitializer()
        {
            _employeeListingService = Substitute.For<IEmployeeListingService>();

            _employeeController = new EmployeeController(ModelMapper.Create(), _employeeListingService);
            _employeeController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetPagedEmployees_ValidValues_ReturnsOk()
        {
            // Arrange
            var args = new EmployeeListingArgsViewModel();

            // Act
            var result = await _employeeController.GetPagedEmployees(args);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPagedEmployees_WhenMethodThrows_ReturnsBadRequest()
        {
            // Arrange
            _employeeListingService
                .GetPagedEmployeesAsync(Arg.Any<EmployeeListingArgsDto>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            var args = new EmployeeListingArgsViewModel();

            // Act
            var result = await _employeeController.GetPagedEmployees(args);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetPagedEmployees_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var args = new EmployeeListingArgsViewModel
            {
                Page = -1
            };

            _employeeController.ModelState.AddModelError("Page", "Invalid value");

            // Act
            var result = await _employeeController.GetPagedEmployees(args);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}
