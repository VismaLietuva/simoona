using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Wall.Likes;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;
using System.Threading.Tasks;

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
            _postController.SetUpControllerForTesting();
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

            _postController.ModelState.AddModelError("Type", "Invalid value");

            // Act
            var result = await _postController.ToggleLike(addLikeViewModel);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
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

            // Act
            var result = await _postController.ToggleLike(addLikeViewModel);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}