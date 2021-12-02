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
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Wall.Likes;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class CommentControllerTests
    {
        private CommentController _commentController;

        private ICommentService _commentService;
        private IWallService _wallService;
        private IPermissionService _permissionService;
        private IAsyncRunner _asyncRunner;

        [SetUp]
        public void TestInitializer()
        {
            _commentService = Substitute.For<ICommentService>();
            _wallService = Substitute.For<IWallService>();
            _permissionService = Substitute.For<IPermissionService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();

            _commentController = new CommentController(ModelMapper.Create(), _commentService, _wallService,
                _permissionService, _asyncRunner);

            _commentController.ControllerContext = Substitute.For<HttpControllerContext>();
            _commentController.Request = new HttpRequestMessage();
            _commentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _commentController.Request.SetConfiguration(new HttpConfiguration());
            _commentController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        [Test]
        public async Task ToggleLike_Should_Return_BadRequest()
        {
            // Arrange
            var addLikeViewModel = new AddLikeViewModel
            {
                Id = 1,
                Type = (LikeTypeEnum)int.MaxValue
            };

            _commentController.Validate(addLikeViewModel);

            // Act
            var httpActionResult = await _commentController.ToggleLike(addLikeViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task ToggleLike_Should_Return_Ok()
        {
            // Arrange
            var addLikeViewModel = new AddLikeViewModel
            {
                Id = 1,
                Type = LikeTypeEnum.Like
            };

            _commentController.Validate(addLikeViewModel);

            // Act
            var httpActionResult = await _commentController.ToggleLike(addLikeViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}