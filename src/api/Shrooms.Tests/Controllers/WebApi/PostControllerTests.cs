using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class PostControllerTests
    {
        private PostController _postController;

        private IPostService _postService;
        private IWallService _wallService;
        private IPermissionService _permissionService;
        private IAsyncRunner _asyncRunner;

        [SetUp]
        public void TestInitializer()
        {
            _postService = Substitute.For<IPostService>();
            _wallService = Substitute.For<IWallService>();
            _permissionService = Substitute.For<IPermissionService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();

            _postController = new PostController(ModelMapper.Create(), _wallService, _postService,
                _permissionService, _asyncRunner);

            _postController.ControllerContext = Substitute.For<HttpControllerContext>();
            _postController.Request = new HttpRequestMessage();
            _postController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _postController.Request.SetConfiguration(new HttpConfiguration());
            _postController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        [Test]
        public async Task ToggleLike_Should_Return_BadRequest()
        {
            // Arrange
            var addLikeDto = new AddLikeDto
            {
                Id = 1,
                LikeType = (LikeTypeEnum)int.MaxValue
            };

            _postController.Validate(addLikeDto);

            // Act
            var httpActionResult = await _postController.ToggleLike(addLikeDto);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ToggleLike_Should_Return_Ok()
        {
            // Arrange
            var addLikeDto = new AddLikeDto
            {
                Id = 1,
                LikeType = LikeTypeEnum.Like
            };

            _postController.Validate(addLikeDto);

            // Act
            var httpActionResult = await _postController.ToggleLike(addLikeDto);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}