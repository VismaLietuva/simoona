using System;
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
    public class VideoLibraryServiceTests
    {
        private const int OtherOrganizationId = 2;

        private IVideoLibraryService _videoLibraryService;
        private DbSet<VideoLibraryItem> _videosDbSet;
        private IUnitOfWork2 _uow;

        private List<VideoLibraryItem> _videos;

        [SetUp]
        public void TestInitializer()
        {
            var allHands = new VideoType
            {
                Id = 1,
                Title = "All-Hands meetings",
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var foreignType = new VideoType
            {
                Id = 2,
                Title = "Foreign type",
                OrganizationId = OtherOrganizationId
            };

            _videos = new List<VideoLibraryItem>
            {
                new()
                {
                    Id = 1,
                    Title = "Older video",
                    Url = "https://drive.google.com/file/d/older/view",
                    Created = new DateTime(2026, 1, 1),
                    VideoTypeId = allHands.Id,
                    VideoType = allHands,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                new()
                {
                    Id = 2,
                    Title = "Newer video",
                    Url = "https://drive.google.com/file/d/newer/view",
                    Description = "Description",
                    PictureId = "picture.jpg",
                    Created = new DateTime(2026, 5, 1),
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                new()
                {
                    Id = 3,
                    Title = "Video of another organization",
                    Url = "https://drive.google.com/file/d/foreign/view",
                    Created = new DateTime(2026, 6, 1),
                    OrganizationId = OtherOrganizationId
                }
            };

            _uow = Substitute.For<IUnitOfWork2>();
            _videosDbSet = _uow.MockDbSetForAsync(_videos);
            _uow.MockDbSetForAsync(new List<VideoType> { allHands, foreignType });

            _videoLibraryService = new VideoLibraryService(_uow);
        }

        [Test]
        public async Task Should_Return_Only_Videos_Of_The_Requesting_Organization()
        {
            var result = (await _videoLibraryService.GetVideosAsync(UserOrg())).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(v => v.Id), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task Should_Return_Newest_Videos_First()
        {
            var result = (await _videoLibraryService.GetVideosAsync(UserOrg())).ToList();

            Assert.That(result.First().Id, Is.EqualTo(2));
            Assert.That(result.Last().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Map_Video_Type_Title_And_Leave_It_Null_When_Untyped()
        {
            var result = (await _videoLibraryService.GetVideosAsync(UserOrg())).ToList();

            var typed = result.First(v => v.Id == 1);
            var untyped = result.First(v => v.Id == 2);

            Assert.That(typed.VideoTypeTitle, Is.EqualTo("All-Hands meetings"));
            Assert.That(untyped.VideoTypeId, Is.Null);
            Assert.That(untyped.VideoTypeTitle, Is.Null);
        }

        [Test]
        public async Task Should_Create_Video_With_Values_From_The_Dto()
        {
            var dto = NewVideoDto();
            dto.VideoTypeId = 1;

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Is<VideoLibraryItem>(v =>
                v.Title == dto.Title &&
                v.Url == dto.Url &&
                v.Description == dto.Description &&
                v.PictureId == dto.PictureId &&
                v.VideoTypeId == 1 &&
                v.CreatedBy == dto.UserId &&
                v.OrganizationId == TestConstants.DefaultOrganizationId));

            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        [Test]
        public async Task Should_Create_Video_Without_A_Type()
        {
            var dto = NewVideoDto();
            dto.VideoTypeId = null;

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Is<VideoLibraryItem>(v => v.VideoTypeId == null));
        }

        [TestCase("javascript:alert(1)")]
        [TestCase("ftp://drive.google.com/file")]
        [TestCase("drive.google.com/file/d/abc/view")]
        [TestCase("/relative/path")]
        [TestCase("not a url at all")]
        [TestCase("")]
        public void Should_Reject_Links_That_Are_Not_Absolute_Http_Urls(string url)
        {
            var dto = NewVideoDto();
            dto.Url = url;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.CreateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.InvalidType));
            _videosDbSet.DidNotReceive().Add(Arg.Any<VideoLibraryItem>());
        }

        [TestCase("http://drive.google.com/file/d/abc/view")]
        [TestCase("https://drive.google.com/file/d/abc/view")]
        public async Task Should_Accept_Absolute_Http_And_Https_Links(string url)
        {
            var dto = NewVideoDto();
            dto.Url = url;

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Is<VideoLibraryItem>(v => v.Url == url));
        }

        [Test]
        public void Should_Not_Create_Video_With_Unknown_Video_Type()
        {
            var dto = NewVideoDto();
            dto.VideoTypeId = 999;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.CreateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
            _videosDbSet.DidNotReceive().Add(Arg.Any<VideoLibraryItem>());
        }

        [Test]
        public void Should_Not_Create_Video_With_A_Video_Type_From_Another_Organization()
        {
            var dto = NewVideoDto();
            dto.VideoTypeId = 2;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.CreateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public async Task Should_Update_Video_Fields()
        {
            var dto = NewVideoDto();
            dto.Id = 1;
            dto.Title = "Updated title";
            dto.Url = "https://drive.google.com/file/d/updated/view";
            dto.Description = "Updated description";
            dto.PictureId = "updated.jpg";
            dto.VideoTypeId = null;

            await _videoLibraryService.UpdateVideoAsync(dto);

            var updated = _videos.First(v => v.Id == 1);

            Assert.That(updated.Title, Is.EqualTo("Updated title"));
            Assert.That(updated.Url, Is.EqualTo("https://drive.google.com/file/d/updated/view"));
            Assert.That(updated.Description, Is.EqualTo("Updated description"));
            Assert.That(updated.PictureId, Is.EqualTo("updated.jpg"));
            Assert.That(updated.VideoTypeId, Is.Null);

            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        [Test]
        public void Should_Not_Update_Video_Belonging_To_Another_Organization()
        {
            var dto = NewVideoDto();
            dto.Id = 3;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.UpdateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public void Should_Not_Update_Video_That_Does_Not_Exist()
        {
            var dto = NewVideoDto();
            dto.Id = 999;

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.UpdateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public async Task Should_Soft_Delete_Video_Instead_Of_Removing_It()
        {
            await _videoLibraryService.RemoveVideoAsync(1, UserOrg());

            Assert.That(_videos.First(v => v.Id == 1).IsDeleted, Is.True);

            _videosDbSet.DidNotReceive().Remove(Arg.Any<VideoLibraryItem>());
            await _uow.Received(1).SaveChangesAsync("testUser");
        }

        [Test]
        public void Should_Not_Delete_Video_Belonging_To_Another_Organization()
        {
            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.RemoveVideoAsync(3, UserOrg()));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
            Assert.That(_videos.First(v => v.Id == 3).IsDeleted, Is.False);
        }

        private static UserAndOrganizationDto UserOrg()
        {
            return new UserAndOrganizationDto
            {
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
        }

        private static VideoLibraryItemDto NewVideoDto()
        {
            return new VideoLibraryItemDto
            {
                Title = "New video",
                Url = "https://drive.google.com/file/d/new/view",
                Description = "Some description",
                PictureId = "picture.jpg",
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
        }
    }
}
