using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models.FilterPresets;
using Shrooms.Tests.ModelMappings;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class FilterPresetControllerTests
    {
        private FilterPresetController _filterPresetController;

        private IFilterPresetService _filterPresetService;

        [SetUp]
        public void TestInitializer()
        {
            _filterPresetService = Substitute.For<IFilterPresetService>();

            _filterPresetController = new FilterPresetController(ModelMapper.Create(), _filterPresetService);

            _filterPresetController.ControllerContext = Substitute.For<HttpControllerContext>();
            _filterPresetController.Request = new HttpRequestMessage();
            _filterPresetController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _filterPresetController.Request.SetConfiguration(new HttpConfiguration());
            _filterPresetController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        [Test]
        public async Task Update_Should_Return_Bad_Request_When_Called_With_Invalid_Page_Type()
        {
            // Arrange
            var updateViewModel = new ManageFilterPresetViewModel
            {
                PageType = (PageType)int.MaxValue,
                PresetsToCreate = Enumerable.Empty<CreateFilterPresetViewModel>(),
                PresetsToDelete = Enumerable.Empty<int>(),
                PresetsToUpdate = Enumerable.Empty<UpdateFilterPresetViewModel>(),
            };

            // Act
            _filterPresetController.Validate(updateViewModel);

            var httpActionResult = await _filterPresetController.Update(updateViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Update_Should_Return_Ok()
        {
            // Arrange
            _filterPresetService.UpdateAsync(Arg.Any<ManageFilterPresetDto>())
                .Returns(new UpdatedFilterPresetDto());

            var updateViewModel = new ManageFilterPresetViewModel();

            // Act
            var httpActionResult = await _filterPresetController.Update(updateViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Update_Should_Return_BadRequest_When_ValidationException_Is_Thrown()
        {
            // Arrange
            _filterPresetService.UpdateAsync(Arg.Any<ManageFilterPresetDto>())
                .Throws(new ValidationException(0));

            var updateViewModel = new ManageFilterPresetViewModel();

            // Act
            var httpActionResult = await _filterPresetController.Update(updateViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetPresets_Should_Return_Ok()
        {
            // Arrange
            _filterPresetService.GetPresetsForPageAsync(Arg.Any<PageType>(), Arg.Any<int>())
                .Returns(Enumerable.Empty<FilterPresetDto>());

            var pageType = PageType.EventReportList;

            // Act
            var httpActionResult = await _filterPresetController.GetPresets(pageType);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetPresets_Should_Return_BadRequest_When_ValidationException_Is_Thrown()
        {
            // Arrange
            _filterPresetService.GetPresetsForPageAsync(Arg.Any<PageType>(), Arg.Any<int>())
                .Throws(new ValidationException(0));

            var pageType = PageType.EventReportList;
            
            // Act
            var httpActionResult = await _filterPresetController.GetPresets(pageType);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetFilters_Should_Return_Ok()
        {
            // Arrange
            _filterPresetService.GetFiltersAsync(Arg.Any<FilterType[]>(), Arg.Any<int>())
                .Returns(Enumerable.Empty<FiltersDto>());
            
            var filterTypes = new[]
            {
                FilterType.Kudos,
                FilterType.Events
            };

            // Act
            var httpActionResult = await _filterPresetController.GetFilters(filterTypes);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetFilters_Should_Return_BadRequest_When_ValidationException_Is_Thrown()
        {
            // Arrange
            _filterPresetService.GetFiltersAsync(Arg.Any<FilterType[]>(), Arg.Any<int>())
                .Throws(new ValidationException(0));

            var filterTypes = new[]
            {
                FilterType.Kudos,
                FilterType.Events
            };

            // Act
            var httpActionResult = await _filterPresetController.GetFilters(filterTypes);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
