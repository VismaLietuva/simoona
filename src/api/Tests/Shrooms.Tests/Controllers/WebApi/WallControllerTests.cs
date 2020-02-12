using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers.Wall;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.UnitTests.ModelMappings;
using Shrooms.WebViewModels.Models.User;
using Shrooms.WebViewModels.Models.Wall;
using Shrooms.WebViewModels.Models.Wall.Moderator;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    public class WallControllerTests
    {
        private WallController _wallController;

        private IWallService _wallService;
        private INotificationService _notificationService;

        [SetUp]
        public void TestInitializer()
        {
            _wallService = Substitute.For<IWallService>();
            _notificationService = Substitute.For<INotificationService>();
            var permissionService = Substitute.For<IPermissionService>();
            permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDTO>(), Arg.Any<string>()).Returns(true);

            _wallController = new WallController(ModelMapper.Create(), _wallService, _notificationService, permissionService);
            _wallController.ControllerContext = Substitute.For<HttpControllerContext>();
            _wallController.Request = new HttpRequestMessage();
            _wallController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _wallController.Request.SetConfiguration(new HttpConfiguration());
            _wallController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public async Task Wall_GetWall_List_Should_Return_View_Model()
        {
            IEnumerable<WallDto> walls = new List<WallDto>
            {
                new WallDto
                {
                    Id = 0,
                    Description = "Description",
                    IsFollowing = false
                },
                new WallDto
                {
                    Id = 1,
                    Description = "Description",
                    IsFollowing = false
                }
            };

            _wallService.GetWallsList(null, WallsListFilter.All).ReturnsForAnyArgs(Task.Run(() => walls));

            var response = await _wallController.GetWallList(WallsListFilter.All);

            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<WallListViewModel>>>(response);
        }

        [Test]
        public async Task Wall_GetWall_Should_Return_Bad_Request()
        {
            var wallId = -1;

            var response = await _wallController.GetWall(wallId);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public async Task Wall_GetWall_Should_Return_View_Model()
        {
            var wallId = 1;
            var wall = new WallDto
            {
                Id = wallId
            };

            _wallService.GetWall(wallId, null).Returns(new WallDto { Type = WallType.UserCreated });
            _wallService.GetWallDetails(0, null).ReturnsForAnyArgs(Task.Run(() => wall));

            var response = await _wallController.GetWall(wallId);

            Assert.IsInstanceOf<OkNegotiatedContentResult<WallListViewModel>>(response);
        }

        [Test]
        public void Wall_JoinWall_Should_Return_Bad_Request()
        {
            var wallId = -1;

            var response = _wallController.JoinWall(wallId);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Wall_JoinWall_Should_Return_View_Model()
        {
            var wallId = 1;
            var userDto = new ApplicationUserMinimalViewModelDto
            {
                Id = "Id"
            };

            _wallService.JoinLeaveWall(wallId, null, null, 0, false).ReturnsForAnyArgs(userDto);

            var response = _wallController.JoinWall(wallId);

            Assert.IsInstanceOf<OkNegotiatedContentResult<ApplicationUserMinimalViewModel>>(response);
        }

        [Test]
        public async Task Wall_GetWallMembers_Should_Return_View_Model()
        {
            var wallId = 0;
            IEnumerable<WallMemberDto> members = new List<WallMemberDto>
            {
                new WallMemberDto
                {
                    Id = "1"
                },
                new WallMemberDto
                {
                    Id = "2"
                }
            };

            _wallService.GetWallMembers(wallId, null).ReturnsForAnyArgs(Task.Run(() => members));

            var response = await _wallController.GetWallMembers(wallId);

            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<WallMemberViewModel>>>(response);
        }

        [Test]
        public async Task Wall_GetPagedWall_Should_Return_Bad_Request()
        {
            var wallId = 0;
            var page = 1;

            var response = await _wallController.GetPagedWall(wallId, page);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public async Task Wall_GetPagedWall_Should_Return_View_Model()
        {
            var wallId = 1;
            var page = 1;

            IEnumerable<PostDTO> posts = new List<PostDTO>
            {
                new PostDTO
                {
                    Id = 0
                },
                new PostDTO
                {
                    Id = 1
                }
            };

            _wallService.GetWall(wallId, null).Returns(new WallDto { Type = WallType.UserCreated });
            _wallService.GetWallPosts(page, WebApiConstants.DefaultPageSize, null, wallId).ReturnsForAnyArgs(Task.Run(() => posts));

            var response = await _wallController.GetPagedWall(wallId, page);

            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedWallViewModel<WallPostViewModel>>>(response);
        }

        [Test]
        public async Task Wall_GetPagedWall_Should_Return_Ok()
        {
            var page = 1;

            var response = await _wallController.GetAllPagedWall(page);

            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedWallViewModel<WallPostViewModel>>>(response);
        }

        [Test]
        public async Task Wall_SearchWall_Should_Return_Bad_Request_If_ModelState_Invalid()
        {
            var page = 1;
            var searchString = "testing";
            _wallController.ModelState.AddModelError("model", "error");

            var response = await _wallController.SearchWall(searchString, page);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public async Task Wall_SearchWall_Should_Return_Ok()
        {
            var searchString = "test";
            var page = 1;

            var response = await _wallController.SearchWall(searchString, page);

            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedWallViewModel<WallPostViewModel>>>(response);
        }

        [Test]
        public async Task Wall_CreateWall_Should_Return_Invalid_Model_State()
        {
            var wall = new CreateWallViewModel
            {
                Name = "test"
            };
            _wallController.ModelState.AddModelError("model", "error");

            var response = await _wallController.CreateWall(wall);

            Assert.IsInstanceOf<InvalidModelStateResult>(response);
        }

        [Test]
        public void Wall_EditWall_Should_Return_Invalid_Model_State()
        {
            var wall = new UpdateWallViewModel
            {
                Name = "test"
            };
            _wallController.ModelState.AddModelError("test", "error");

            var response = _wallController.EditWall(wall);

            Assert.IsInstanceOf<InvalidModelStateResult>(response);
        }

        [Test]
        public void Wall_EditWall_Should_Return_Ok()
        {
            IEnumerable<ModeratorViewModel> moderators = new List<ModeratorViewModel>()
            {
                new ModeratorViewModel
                {
                    Id = "test",
                    FullName = "test"
                }
            };

            var wall = new UpdateWallViewModel
            {
                Id = 1,
                Name = "test",
                Description = "test",
                Logo = "test",
                Moderators = moderators
            };

            var response = _wallController.EditWall(wall);

            Assert.IsInstanceOf<OkResult>(response);
        }

        [Test]
        public void Wall_DeleteWall_Should_Return_Bad_Request()
        {
            var id = 0;

            var response = _wallController.DeleteWall(id);

            Assert.IsInstanceOf<BadRequestResult>(response);
        }

        [Test]
        public void Wall_DeleteWall_Should_Return_Ok()
        {
            var id = 1;

            var response = _wallController.DeleteWall(id);

            Assert.IsInstanceOf<OkResult>(response);
        }
    }
}
