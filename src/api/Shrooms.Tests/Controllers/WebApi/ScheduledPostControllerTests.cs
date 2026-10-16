using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.ScheduledPosts;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class ScheduledPostControllerTests
    {
        private ScheduledPostController _controller;
        private IScheduledPostService _scheduledPostService;
        private IWallService _wallService;
        private IPermissionService _permissionService;

        [SetUp]
        public void TestInitializer()
        {
            _scheduledPostService = Substitute.For<IScheduledPostService>();
            _wallService = Substitute.For<IWallService>();
            _permissionService = Substitute.For<IPermissionService>();

            _controller = new ScheduledPostController(_scheduledPostService, _wallService, _permissionService, Substitute.For<IAsyncRunner>());
            _controller.SetUpControllerForTesting();
        }

        [Test]
        public async Task Create_Should_Return_BadRequest_When_The_Wall_Is_Missing_For_An_Event_Only_User()
        {
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), BasicPermissions.Post).Returns(false);

            var result = await _controller.Create(new NewScheduledPostDto { WallId = 404 });

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            await _scheduledPostService.DidNotReceive().CreateAsync(Arg.Any<NewScheduledPostDto>());
        }
    }
}
