using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.VideoLibrary;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class VideoTypeServiceTests
    {
        private const int OtherOrganizationId = 2;

        private IVideoTypeService _videoTypeService;
        private DbSet<VideoType> _videoTypesDbSet;
        private IUnitOfWork2 _uow;

        private List<VideoType> _videoTypes;
        private List<VideoLibraryItem> _videos;

        [SetUp]
        public void TestInitializer()
        {
            var aiHubs = new VideoType
            {
                Id = 1,
                Title = "AI Hubs",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var allHands = new VideoType
            {
                Id = 2,
                Title = "All-Hands meetings",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var unused = new VideoType
            {
                Id = 3,
                Title = "Unused type",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var foreignType = new VideoType
            {
                Id = 4,
                Title = "Foreign type",
                OrganizationId = OtherOrganizationId
            };

            _videos = new List<VideoLibraryItem>
            {
                new()
                {
                    Id = 1,
                    Title = "Live video",
                    VideoTypeId = aiHubs.Id,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                new()
                {
                    Id = 2,
                    Title = "Deleted video",
                    VideoTypeId = aiHubs.Id,
                    IsDeleted = true,
                    OrganizationId = TestConstants.DefaultOrganizationId
                }
            };

            aiHubs.Videos = _videos;
            allHands.Videos = new List<VideoLibraryItem>();
            unused.Videos = new List<VideoLibraryItem>();
            foreignType.Videos = new List<VideoLibraryItem>();

            _videoTypes = new List<VideoType> { allHands, aiHubs, unused, foreignType };

            _uow = Substitute.For<IUnitOfWork2>();
            _videoTypesDbSet = _uow.MockDbSetForAsync(_videoTypes);
            _uow.MockDbSetForAsync(_videos);

            _videoTypeService = new VideoTypeService(_uow);
        }

        [Test]
        public async Task Should_Return_Only_Video_Types_Of_The_Requesting_Organization()
        {
            var result = (await _videoTypeService.GetVideoTypesAsync(UserOrg())).ToList();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Select(t => t.Id), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public async Task Should_Return_Video_Types_Ordered_By_Title()
        {
            var result = (await _videoTypeService.GetVideoTypesAsync(UserOrg())).ToList();

            Assert.That(result.Select(t => t.Title), Is.EqualTo(new[] { "AI Hubs", "All-Hands meetings", "Unused type" }));
        }

        [Test]
        public async Task Should_Not_Count_Deleted_Videos_Towards_The_Video_Type_Usage()
        {
            var result = (await _videoTypeService.GetVideoTypesAsync(UserOrg())).ToList();

            Assert.That(result.First(t => t.Id == 1).VideoCount, Is.EqualTo(1));
            Assert.That(result.First(t => t.Id == 3).VideoCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Create_Video_Type()
        {
            var dto = NewVideoTypeDto("Brand new type");

            await _videoTypeService.CreateVideoTypeAsync(dto);

            _videoTypesDbSet.Received(1).Add(Arg.Is<VideoType>(t =>
                t.Title == "Brand new type" &&
                t.CreatedBy == dto.UserId &&
                t.OrganizationId == TestConstants.DefaultOrganizationId));

            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        [Test]
        public void Should_Not_Create_Video_Type_With_A_Title_That_Is_Already_Taken()
        {
            var dto = NewVideoTypeDto("AI Hubs");

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.CreateVideoTypeAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
            _videoTypesDbSet.DidNotReceive().Add(Arg.Any<VideoType>());
        }

        [Test]
        public async Task Should_Allow_A_Title_Already_Used_By_Another_Organization()
        {
            var dto = NewVideoTypeDto("Foreign type");

            await _videoTypeService.CreateVideoTypeAsync(dto);

            _videoTypesDbSet.Received(1).Add(Arg.Is<VideoType>(t => t.Title == "Foreign type"));
        }

        [Test]
        public async Task Should_Update_Video_Type_Title()
        {
            var dto = NewVideoTypeDto("Renamed type");
            dto.Id = 1;

            await _videoTypeService.UpdateVideoTypeAsync(dto);

            Assert.That(_videoTypes.First(t => t.Id == 1).Title, Is.EqualTo("Renamed type"));
            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        [Test]
        public async Task Should_Allow_Saving_A_Video_Type_Under_Its_Own_Unchanged_Title()
        {
            var dto = NewVideoTypeDto("AI Hubs");
            dto.Id = 1;

            await _videoTypeService.UpdateVideoTypeAsync(dto);

            Assert.That(_videoTypes.First(t => t.Id == 1).Title, Is.EqualTo("AI Hubs"));
        }

        [Test]
        public void Should_Not_Rename_A_Video_Type_To_A_Title_Another_Type_Already_Uses()
        {
            var dto = NewVideoTypeDto("All-Hands meetings");
            dto.Id = 1;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.UpdateVideoTypeAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
            Assert.That(_videoTypes.First(t => t.Id == 1).Title, Is.EqualTo("AI Hubs"));
        }

        [Test]
        public void Should_Not_Update_Video_Type_Belonging_To_Another_Organization()
        {
            var dto = NewVideoTypeDto("Renamed foreign type");
            dto.Id = 4;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.UpdateVideoTypeAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public async Task Should_Soft_Delete_Unused_Video_Type()
        {
            await _videoTypeService.RemoveVideoTypeAsync(3, UserOrg());

            Assert.That(_videoTypes.First(t => t.Id == 3).IsDeleted, Is.True);

            _videoTypesDbSet.DidNotReceive().Remove(Arg.Any<VideoType>());
            await _uow.Received(1).SaveChangesAsync("testUser");
        }

        [Test]
        public void Should_Not_Delete_Video_Type_That_Is_Still_Assigned_To_A_Video()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.RemoveVideoTypeAsync(1, UserOrg()));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
            Assert.That(_videoTypes.First(t => t.Id == 1).IsDeleted, Is.False);
        }

        [Test]
        public void Should_Not_Delete_Video_Type_Belonging_To_Another_Organization()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.RemoveVideoTypeAsync(4, UserOrg()));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
            Assert.That(_videoTypes.First(t => t.Id == 4).IsDeleted, Is.False);
        }

        [Test]
        public void Should_Not_Delete_Video_Type_That_Does_Not_Exist()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoTypeService.RemoveVideoTypeAsync(999, UserOrg()));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        private static UserAndOrganizationDto UserOrg()
        {
            return new UserAndOrganizationDto
            {
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
        }

        private static VideoTypeDto NewVideoTypeDto(string title)
        {
            return new VideoTypeDto
            {
                Title = title,
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
        }
    }
}
