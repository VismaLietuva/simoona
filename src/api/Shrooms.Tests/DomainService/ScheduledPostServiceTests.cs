using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.ScheduledPosts;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class ScheduledPostServiceTests
    {
        private const string Author = "author";

        private static readonly DateTime _now = new DateTime(2026, 10, 16, 8, 0, 0, DateTimeKind.Utc);

        private ShroomsDbContext _dbContext;
        private IPostService _postService;
        private IJobScheduler _jobScheduler;
        private ScheduledPostService _service;
        private Wall _wall;
        private int _jobCounter;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ShroomsDbContext(options);

            _wall = new Wall
            {
                Name = "General",
                OrganizationId = 1,
                Type = WallType.UserCreated,
                Access = WallAccess.Public
            };
            _dbContext.Set<Wall>().Add(_wall);
            _dbContext.SaveChanges(false);

            _postService = Substitute.For<IPostService>();
            _postService.CreateNewPostAsync(Arg.Any<NewPostDto>())
                .Returns(call => new NewlyCreatedPostDto { Id = 42, WallId = call.Arg<NewPostDto>().WallId, User = new UserDto { UserId = Author } });

            _jobCounter = 0;
            _jobScheduler = Substitute.For<IJobScheduler>();
            _jobScheduler.Schedule(Arg.Any<Expression<Func<IScheduledPostPublisher, Task>>>(), Arg.Any<DateTimeOffset>())
                .Returns(_ => $"job-{++_jobCounter}");

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(_now);

            _service = new ScheduledPostService(new UnitOfWork2(_dbContext), _postService, _jobScheduler, systemClock);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Should_Store_The_Post_And_Schedule_A_Job_For_Its_Time()
        {
            var scheduledFor = _now.AddDays(1);

            var created = await _service.CreateAsync(NewPost("Hello", scheduledFor));

            var stored = await _dbContext.Set<ScheduledPost>().SingleAsync();
            Assert.Multiple(() =>
            {
                Assert.That(stored.JobId, Is.EqualTo("job-1"));
                Assert.That(stored.AuthorId, Is.EqualTo(Author));
                Assert.That(created.WallName, Is.EqualTo("General"));
                Assert.That(created.ScheduledFor, Is.EqualTo(scheduledFor));
            });
            _jobScheduler.Received(1).Schedule(
                Arg.Any<Expression<Func<IScheduledPostPublisher, Task>>>(),
                new DateTimeOffset(scheduledFor, TimeSpan.Zero));
        }

        [Test]
        public void Should_Reject_A_Time_In_The_Past()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(NewPost("Hello", _now.AddMinutes(-1))));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ScheduledPostTimeInPast));
        }

        [Test]
        public void Should_Reject_A_Post_Without_Message_Or_Image()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(NewPost("  ", _now.AddDays(1))));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ScheduledPostContentInvalid));
        }

        [Test]
        public async Task Should_List_Only_The_Authors_Posts_On_The_Wall_Soonest_First()
        {
            await _service.CreateAsync(NewPost("Later", _now.AddDays(2)));
            await _service.CreateAsync(NewPost("Sooner", _now.AddDays(1)));
            await _service.CreateAsync(NewPost("Someone else's", _now.AddDays(1), "other"));

            var posts = (await _service.GetScheduledPostsAsync(_wall.Id, UserOrg(Author))).ToList();

            Assert.That(posts.Select(post => post.MessageBody), Is.EqualTo(new[] { "Sooner", "Later" }));
        }

        [Test]
        public async Task Should_Replace_The_Job_When_Rescheduled()
        {
            var created = await _service.CreateAsync(NewPost("Hello", _now.AddDays(1)));

            await _service.EditAsync(EditPost(created.Id, "Hello", _now.AddDays(3)));

            var stored = await _dbContext.Set<ScheduledPost>().SingleAsync();
            Assert.Multiple(() =>
            {
                Assert.That(stored.JobId, Is.EqualTo("job-2"));
                Assert.That(stored.ScheduledFor, Is.EqualTo(_now.AddDays(3)));
            });
            _jobScheduler.Received(1).Delete("job-1");
        }

        [Test]
        public async Task Should_Keep_The_Job_When_Only_The_Message_Changes()
        {
            var created = await _service.CreateAsync(NewPost("Hello", _now.AddDays(1)));

            await _service.EditAsync(EditPost(created.Id, "Edited", created.ScheduledFor));

            var stored = await _dbContext.Set<ScheduledPost>().SingleAsync();
            Assert.Multiple(() =>
            {
                Assert.That(stored.MessageBody, Is.EqualTo("Edited"));
                Assert.That(stored.JobId, Is.EqualTo("job-1"));
            });
            _jobScheduler.DidNotReceive().Delete(Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Let_Another_User_Edit_The_Post()
        {
            var created = await _service.CreateAsync(NewPost("Hello", _now.AddDays(1)));
            var edit = EditPost(created.Id, "Hijacked", created.ScheduledFor);
            edit.UserId = "other";

            var exception = Assert.ThrowsAsync<ValidationException>(() => _service.EditAsync(edit));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public async Task Should_Remove_The_Post_And_Its_Job_When_Deleted()
        {
            var created = await _service.CreateAsync(NewPost("Hello", _now.AddDays(1)));

            await _service.DeleteAsync(created.Id, UserOrg(Author));

            Assert.That(await _dbContext.Set<ScheduledPost>().AnyAsync(), Is.False);
            _jobScheduler.Received(1).Delete("job-1");
        }

        [Test]
        public async Task Should_Publish_A_Due_Post_As_The_Author()
        {
            var id = await AddDuePostAsync(_now);

            var published = await _service.PublishDueAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(published.Id, Is.EqualTo(42));
                Assert.That(_dbContext.Set<ScheduledPost>().Any(), Is.False);
            });
            await _postService.Received(1).CreateNewPostAsync(Arg.Is<NewPostDto>(dto =>
                dto.UserId == Author &&
                dto.OrganizationId == 1 &&
                dto.WallId == _wall.Id &&
                dto.MessageBody == "Hello"));
        }

        [Test]
        public async Task Should_Skip_A_Post_That_Was_Rescheduled_Later()
        {
            var id = await AddDuePostAsync(_now.AddHours(1));

            var published = await _service.PublishDueAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(published, Is.Null);
                Assert.That(_dbContext.Set<ScheduledPost>().Any(), Is.True);
            });
            await _postService.DidNotReceive().CreateNewPostAsync(Arg.Any<NewPostDto>());
        }

        [Test]
        public async Task Should_Drop_A_Due_Post_Whose_Wall_Is_Gone()
        {
            var id = await AddDuePostAsync(_now);
            _postService.CreateNewPostAsync(Arg.Any<NewPostDto>())
                .Throws(new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found"));

            var published = await _service.PublishDueAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(published, Is.Null);
                Assert.That(_dbContext.Set<ScheduledPost>().Any(), Is.False);
            });
        }

        private async Task<int> AddDuePostAsync(DateTime scheduledFor)
        {
            var scheduledPost = new ScheduledPost
            {
                OrganizationId = 1,
                AuthorId = Author,
                WallId = _wall.Id,
                MessageBody = "Hello",
                Images = new ImageCollection(),
                ScheduledFor = scheduledFor,
                JobId = "job-due"
            };
            _dbContext.Set<ScheduledPost>().Add(scheduledPost);
            await _dbContext.SaveChangesAsync(false);
            _dbContext.ChangeTracker.Clear();

            return scheduledPost.Id;
        }

        private NewScheduledPostDto NewPost(string messageBody, DateTime scheduledFor, string userId = Author)
        {
            return new NewScheduledPostDto
            {
                UserId = userId,
                OrganizationId = 1,
                WallId = _wall.Id,
                MessageBody = messageBody,
                ScheduledFor = scheduledFor
            };
        }

        private static EditScheduledPostDto EditPost(int id, string messageBody, DateTime scheduledFor)
        {
            return new EditScheduledPostDto
            {
                Id = id,
                UserId = Author,
                OrganizationId = 1,
                MessageBody = messageBody,
                ScheduledFor = scheduledFor
            };
        }

        private static UserAndOrganizationDto UserOrg(string userId)
        {
            return new UserAndOrganizationDto { UserId = userId, OrganizationId = 1 };
        }
    }
}
