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
using Shrooms.Contracts.ViewModels;
using Shrooms.Presentation.Common.Filters;
using X.PagedList;
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
        public async Task List_Should_Return_A_Page_Of_Mapped_Videos()
        {
            StubPage(totalItemCount: 30, pageSize: 24);

            var result = await _videoLibraryController.List(new VideoLibraryListArgsViewModel());

            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var paged = ((OkObjectResult)result).Value as PagedViewModel<VideoLibraryItemViewModel>;

            Assert.That(paged, Is.Not.Null);
            Assert.That(paged.PagedList.Single().Title, Is.EqualTo("All-Hands April"));
            Assert.That(paged.PagedList.Single().VideoTypeTitle, Is.EqualTo("All-Hands meetings"));
        }

        [Test]
        public async Task List_Should_Report_The_Whole_Library_Size_So_The_Caller_Can_Page()
        {
            StubPage(totalItemCount: 30, pageSize: 24);

            var result = await _videoLibraryController.List(new VideoLibraryListArgsViewModel());
            var paged = ((OkObjectResult)result).Value as PagedViewModel<VideoLibraryItemViewModel>;

            Assert.That(paged.ItemCount, Is.EqualTo(30));
            Assert.That(paged.PageCount, Is.EqualTo(2));
            Assert.That(paged.PageSize, Is.EqualTo(24));
        }

        [Test]
        public async Task List_Should_Pass_The_Query_Arguments_Through_To_The_Service()
        {
            StubPage(totalItemCount: 1, pageSize: 5);

            await _videoLibraryController.List(new VideoLibraryListArgsViewModel
            {
                Search = "all-hands",
                VideoTypeId = 3,
                Page = 2,
                PageSize = 5
            });

            await _videoLibraryService.Received(1).GetVideosAsync(Arg.Is<VideoLibraryListArgsDto>(args =>
                args.Search == "all-hands" &&
                args.VideoTypeId == 3 &&
                args.Page == 2 &&
                args.PageSize == 5 &&
                args.UserId == "1" &&
                args.OrganizationId == TestConstants.DefaultOrganizationId));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task List_Should_Reject_A_Page_That_Cannot_Exist(int page)
        {
            var argsViewModel = new VideoLibraryListArgsViewModel { Page = page };
            _videoLibraryController.Validate(argsViewModel);

            var result = await _videoLibraryController.List(argsViewModel);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoLibraryService.DidNotReceive().GetVideosAsync(Arg.Any<VideoLibraryListArgsDto>());
        }

        [Test]
        public async Task List_Should_Refuse_To_Serve_An_Unbounded_Page_Size()
        {
            var argsViewModel = new VideoLibraryListArgsViewModel
            {
                PageSize = VideoLibraryListArgsViewModel.MaxPageSize + 1
            };
            _videoLibraryController.Validate(argsViewModel);

            var result = await _videoLibraryController.List(argsViewModel);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoLibraryService.DidNotReceive().GetVideosAsync(Arg.Any<VideoLibraryListArgsDto>());
        }

        [Test]
        public async Task Filters_Should_Return_Whole_Library_Counts()
        {
            _videoLibraryService.GetFiltersAsync(Arg.Any<UserAndOrganizationDto>())
                .Returns(new VideoLibraryFiltersDto
                {
                    Types = new[] { new VideoTypeDto { Id = 3, Title = "All-Hands meetings", VideoCount = 7 } },
                    UncategorisedCount = 2,
                    TotalCount = 9
                });

            var result = await _videoLibraryController.Filters();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var filters = ((OkObjectResult)result).Value as VideoLibraryFiltersViewModel;

            Assert.That(filters.TotalCount, Is.EqualTo(9));
            Assert.That(filters.UncategorisedCount, Is.EqualTo(2));
            Assert.That(filters.Types.Single().VideoCount, Is.EqualTo(7));
        }

        private void StubPage(int totalItemCount, int pageSize)
        {
            var video = new VideoLibraryItemDto
            {
                Id = 1,
                Title = "All-Hands April",
                Url = "https://drive.google.com/file/d/abc/view",
                VideoTypeId = 3,
                VideoTypeTitle = "All-Hands meetings",
                Created = new DateTime(2026, 4, 1)
            };

            _videoLibraryService.GetVideosAsync(Arg.Any<VideoLibraryListArgsDto>())
                .Returns(new StaticPagedList<VideoLibraryItemDto>(new[] { video }, 1, pageSize, totalItemCount));
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

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Update_Should_Reject_An_Id_That_Cannot_Exist(int id)
        {
            var viewModel = new VideoLibraryItemViewModel
            {
                Id = id,
                Title = "Updated",
                Url = "https://drive.google.com/file/d/abc/view"
            };
            _videoLibraryController.Validate(viewModel);

            var result = await _videoLibraryController.Update(viewModel);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _videoLibraryService.DidNotReceive().UpdateVideoAsync(Arg.Any<VideoLibraryItemDto>());
        }

        [Test]
        public async Task Update_Should_Accept_A_Real_Id()
        {
            var viewModel = new VideoLibraryItemViewModel
            {
                Id = 1,
                Title = "Updated",
                Url = "https://drive.google.com/file/d/abc/view"
            };
            _videoLibraryController.Validate(viewModel);

            var result = await _videoLibraryController.Update(viewModel);

            Assert.That(result, Is.InstanceOf<OkResult>());
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
