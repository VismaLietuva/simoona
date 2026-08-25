using System;
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
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class VideoLibraryControllerTests
    {
        private VideoLibraryController _videoLibraryController;
        private IVideoLibraryService _videoLibraryService;

        [SetUp]
        public void TestInitializer()
        {
            _videoLibraryService = Substitute.For<IVideoLibraryService>();

            _videoLibraryController = new VideoLibraryController(ModelMapper.Create(), _videoLibraryService);
            _videoLibraryController.SetUpControllerForTesting();
        }

        [Test]
        public async Task List_Should_Return_Mapped_Videos()
        {
            _videoLibraryService.GetVideosAsync(Arg.Any<UserAndOrganizationDto>())
                .Returns(new List<VideoLibraryItemDto>
                {
                    new()
                    {
                        Id = 1,
                        Title = "All-Hands April",
                        Url = "https://drive.google.com/file/d/abc/view",
                        VideoTypeId = 3,
                        VideoTypeTitle = "All-Hands meetings",
                        Created = new DateTime(2026, 4, 1)
                    }
                });

            var result = await _videoLibraryController.List();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var videos = ((OkObjectResult)result).Value as IEnumerable<VideoLibraryItemViewModel>;

            Assert.That(videos, Is.Not.Null);
            Assert.That(videos.Single().Title, Is.EqualTo("All-Hands April"));
            Assert.That(videos.Single().VideoTypeTitle, Is.EqualTo("All-Hands meetings"));
        }

        [Test]
        public async Task Create_Should_Return_BadRequest_If_Invalid_ModelState()
        {
            _videoLibraryController.ModelState.AddModelError("Url", "error");

            var result = await _videoLibraryController.Create(new NewVideoLibraryItemViewModel());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoLibraryService.DidNotReceive().CreateVideoAsync(Arg.Any<VideoLibraryItemDto>());
        }

        [Test]
        public async Task Create_Should_Pass_The_Current_User_And_Organization_To_The_Service()
        {
            var viewModel = new NewVideoLibraryItemViewModel
            {
                Title = "New video",
                Url = "https://drive.google.com/file/d/abc/view",
                VideoTypeId = 2
            };

            var result = await _videoLibraryController.Create(viewModel);

            Assert.That(result, Is.InstanceOf<OkResult>());

            await _videoLibraryService.Received(1).CreateVideoAsync(Arg.Is<VideoLibraryItemDto>(dto =>
                dto.Title == "New video" &&
                dto.Url == "https://drive.google.com/file/d/abc/view" &&
                dto.VideoTypeId == 2 &&
                dto.UserId == "1" &&
                dto.OrganizationId == TestConstants.DefaultOrganizationId));
        }

        [Test]
        public async Task Create_Should_Surface_The_Validation_Error_From_The_Service()
        {
            _videoLibraryService.CreateVideoAsync(Arg.Any<VideoLibraryItemDto>())
                .Returns<Task>(_ => throw new ValidationException(ErrorCodes.InvalidType, "bad link"));

            var result = await _videoLibraryController.Create(new NewVideoLibraryItemViewModel
            {
                Title = "New video",
                Url = "not-a-url"
            });

            AssertBadRequestWithErrorCode(result, ErrorCodes.InvalidType);
        }

        [Test]
        public async Task Update_Should_Return_BadRequest_If_Invalid_ModelState()
        {
            _videoLibraryController.ModelState.AddModelError("Title", "error");

            var result = await _videoLibraryController.Update(new VideoLibraryItemViewModel());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoLibraryService.DidNotReceive().UpdateVideoAsync(Arg.Any<VideoLibraryItemDto>());
        }

        [Test]
        public async Task Update_Should_Return_Ok()
        {
            var result = await _videoLibraryController.Update(new VideoLibraryItemViewModel
            {
                Id = 1,
                Title = "Updated",
                Url = "https://drive.google.com/file/d/abc/view"
            });

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _videoLibraryService.Received(1).UpdateVideoAsync(Arg.Is<VideoLibraryItemDto>(dto => dto.Id == 1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Delete_Should_Return_BadRequest_For_Ids_That_Cannot_Exist(int id)
        {
            var result = await _videoLibraryController.Delete(id);

            Assert.That(result, Is.InstanceOf<BadRequestResult>());
            await _videoLibraryService.DidNotReceive().RemoveVideoAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>());
        }

        [Test]
        public async Task Delete_Should_Return_Ok()
        {
            var result = await _videoLibraryController.Delete(1);

            Assert.That(result, Is.InstanceOf<OkResult>());
            await _videoLibraryService.Received(1).RemoveVideoAsync(1, Arg.Is<UserAndOrganizationDto>(userOrg =>
                userOrg.UserId == "1" && userOrg.OrganizationId == TestConstants.DefaultOrganizationId));
        }

        [Test]
        public async Task Delete_Should_Surface_The_Validation_Error_From_The_Service()
        {
            _videoLibraryService.RemoveVideoAsync(Arg.Any<int>(), Arg.Any<UserAndOrganizationDto>())
                .Returns<Task>(_ => throw new ValidationException(ErrorCodes.ContentDoesNotExist, "not found"));

            var result = await _videoLibraryController.Delete(1);

            AssertBadRequestWithErrorCode(result, ErrorCodes.ContentDoesNotExist);
        }

        [Test]
        public void List_Should_Not_Be_Restricted_To_Administrators()
        {
            var list = typeof(VideoLibraryController).GetMethod(nameof(VideoLibraryController.List));

            Assert.That(list.GetCustomAttribute<PermissionAnyOfAuthorizeAttribute>(), Is.Not.Null);
            Assert.That(list.GetCustomAttribute<PermissionAuthorizeAttribute>(), Is.Null);
        }

        [TestCase(nameof(VideoLibraryController.Create))]
        [TestCase(nameof(VideoLibraryController.Update))]
        [TestCase(nameof(VideoLibraryController.Delete))]
        public void Mutating_Actions_Should_Require_The_Administration_Permission(string actionName)
        {
            var action = typeof(VideoLibraryController).GetMethod(actionName);
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
