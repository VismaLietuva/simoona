using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.VideoLibrary;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class VideoTypeControllerTests
    {
        private VideoTypeController _videoTypeController;
        private IVideoTypeService _videoTypeService;

        [SetUp]
        public void TestInitializer()
        {
            _videoTypeService = Substitute.For<IVideoTypeService>();

            _videoTypeController = new VideoTypeController(ModelMapper.Create(), _videoTypeService);
            _videoTypeController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetVideoTypes_Should_Return_Mapped_Types_With_Their_Usage_Count()
        {
            _videoTypeService.GetVideoTypesAsync(Arg.Any<UserAndOrganizationDto>())
                .Returns(new List<VideoTypeDto>
                {
                    new() { Id = 1, Title = "AI Hubs", VideoCount = 4 }
                });

            var result = await _videoTypeController.GetVideoTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var videoTypes = ((OkObjectResult)result).Value as IEnumerable<VideoTypeViewModel>;

            Assert.That(videoTypes, Is.Not.Null);
            Assert.That(videoTypes.Single().Title, Is.EqualTo("AI Hubs"));
            Assert.That(videoTypes.Single().VideoCount, Is.EqualTo(4));
        }

        [Test]
        public async Task Create_Should_Return_BadRequest_If_Invalid_ModelState()
        {
            _videoTypeController.ModelState.AddModelError("Title", "error");

            var result = await _videoTypeController.Create(new NewVideoTypeViewModel());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoTypeService.DidNotReceive().CreateVideoTypeAsync(Arg.Any<VideoTypeDto>());
        }

        [Test]
        public async Task Create_Should_Pass_The_Current_User_And_Organization_To_The_Service()
        {
            var result = await _videoTypeController.Create(new NewVideoTypeViewModel { Title = "AI Hubs" });

            Assert.That(result, Is.InstanceOf<OkResult>());

            await _videoTypeService.Received(1).CreateVideoTypeAsync(Arg.Is<VideoTypeDto>(dto =>
                dto.Title == "AI Hubs" &&
                dto.UserId == "1" &&
                dto.OrganizationId == TestConstants.DefaultOrganizationId));
        }

        [Test]
        public async Task Create_Should_Surface_The_Duplicate_Title_Error_From_The_Service()
        {
            _videoTypeService.CreateVideoTypeAsync(Arg.Any<VideoTypeDto>())
                .Returns<Task>(_ => throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "duplicate"));

            var result = await _videoTypeController.Create(new NewVideoTypeViewModel { Title = "AI Hubs" });

            AssertBadRequestWithErrorCode(result, ErrorCodes.DuplicatesIntolerable);
        }

        [Test]
        public async Task Update_Should_Return_BadRequest_If_Invalid_ModelState()
        {
            _videoTypeController.ModelState.AddModelError("Title", "error");

            var result = await _videoTypeController.Update(new VideoTypeViewModel());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoTypeService.DidNotReceive().UpdateVideoTypeAsync(Arg.Any<VideoTypeDto>());
        }

        [Test]
        public async Task Update_Should_Return_Ok()
        {
            var result = await _videoTypeController.Update(new VideoTypeViewModel { Id = 1, Title = "Renamed" });

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _videoTypeService.Received(1).UpdateVideoTypeAsync(Arg.Is<VideoTypeDto>(dto =>
                dto.Id == 1 && dto.Title == "Renamed"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Delete_Should_Return_BadRequest_For_Ids_That_Cannot_Exist(int id)
        {
            var result = await _videoTypeController.Delete(id);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
            await _videoTypeService.DidNotReceive().RemoveVideoTypeAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task Delete_Should_Return_Ok()
        {
            var result = await _videoTypeController.Delete(1);

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _videoTypeService.Received(1).RemoveVideoTypeAsync(1, Arg.Is<UserAndOrganizationDto>(userOrg =>
                userOrg.UserId == "1" && userOrg.OrganizationId == TestConstants.DefaultOrganizationId));
        }

        [Test]
        public async Task Delete_Should_Surface_The_Still_In_Use_Error_From_The_Service()
        {
            _videoTypeService.RemoveVideoTypeAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns<Task>(_ => throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "still in use"));

            var result = await _videoTypeController.Delete(1);

            AssertBadRequestWithErrorCode(result, ErrorCodes.DuplicatesIntolerable);
        }

        [TestCase(nameof(VideoTypeController.GetVideoTypes))]
        [TestCase(nameof(VideoTypeController.Create))]
        [TestCase(nameof(VideoTypeController.Update))]
        [TestCase(nameof(VideoTypeController.Delete))]
        public void Every_Action_Should_Require_The_Administration_Permission(string actionName)
        {
            var action = typeof(VideoTypeController).GetMethod(actionName);
            var attribute = action.GetCustomAttribute<PermissionAuthorizeAttribute>();

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Permission, Is.EqualTo(AdministrationPermissions.VideoLibrary));
        }

        private static void AssertBadRequestWithErrorCode(IActionResult result, int expectedErrorCode)
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var value = ((BadRequestObjectResult)result).Value;
            var errorCode = value.GetType().GetProperty("ErrorCode").GetValue(value);

            Assert.That(errorCode, Is.EqualTo(expectedErrorCode));
        }
    }
}
