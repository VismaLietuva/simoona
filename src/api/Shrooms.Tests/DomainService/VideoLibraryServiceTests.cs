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
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.VideoLibrary;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class VideoLibraryServiceTests
    {
        private const int OtherOrganizationId = 2;

        private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0);
        private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

        private IVideoLibraryService _videoLibraryService;
        private DbSet<VideoLibraryItem> _videosDbSet;
        private IUnitOfWork2 _uow;
        private IPictureService _pictureService;

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
                    RecordedOn = new DateOnly(2026, 1, 1),
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
                    RecordedOn = new DateOnly(2026, 5, 1),
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                new()
                {
                    Id = 3,
                    Title = "Video of another organization",
                    Url = "https://drive.google.com/file/d/foreign/view",
                    Created = new DateTime(2026, 6, 1),
                    RecordedOn = new DateOnly(2026, 6, 1),
                    OrganizationId = OtherOrganizationId
                }
            };

            allHands.Videos = _videos.Where(v => v.VideoTypeId == allHands.Id).ToList();
            foreignType.Videos = _videos.Where(v => v.VideoTypeId == foreignType.Id).ToList();

            _uow = Substitute.For<IUnitOfWork2>();
            _videosDbSet = _uow.MockDbSetForAsync(_videos);
            _uow.MockDbSetForAsync(new List<VideoType> { allHands, foreignType });

            _pictureService = Substitute.For<IPictureService>();

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(Now);

            _videoLibraryService = new VideoLibraryService(_uow, _pictureService, systemClock);
        }

        [Test]
        public async Task Should_Return_Only_Videos_Of_The_Requesting_Organization()
        {
            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(v => v.Id), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task Should_Return_Newest_Videos_First()
        {
            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            Assert.That(result.First().Id, Is.EqualTo(2));
            Assert.That(result.Last().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Place_An_Old_Recording_Added_Recently_By_Its_Recording_Date()
        {
            _videos.First(v => v.Id == 2).RecordedOn = new DateOnly(2025, 6, 1);

            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            Assert.That(result.Select(v => v.Id), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task Should_Order_Videos_Recorded_The_Same_Day_By_Added_Date()
        {
            _videos.First(v => v.Id == 1).RecordedOn = new DateOnly(2025, 6, 1);
            _videos.First(v => v.Id == 2).RecordedOn = new DateOnly(2025, 6, 1);

            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            Assert.That(result.Select(v => v.Id), Is.EqualTo(new[] { 2, 1 }));
        }

        [Test]
        public async Task Should_Filter_By_Recording_Date_Including_Both_Ends()
        {
            _videos.First(v => v.Id == 2).RecordedOn = new DateOnly(2025, 6, 1);

            var result = await _videoLibraryService.GetVideosAsync(
                ListArgs(from: new DateOnly(2025, 6, 1), to: new DateOnly(2026, 1, 1)));

            Assert.That(result.Select(v => v.Id), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(result.TotalItemCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Filter_By_Recording_Date_With_Only_One_End()
        {
            var since = await _videoLibraryService.GetVideosAsync(ListArgs(from: new DateOnly(2026, 2, 1)));
            var until = await _videoLibraryService.GetVideosAsync(ListArgs(to: new DateOnly(2026, 2, 1)));

            Assert.That(since.Select(v => v.Id), Is.EqualTo(new[] { 2 }));
            Assert.That(until.Select(v => v.Id), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public async Task Should_Return_Recording_Date()
        {
            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            Assert.That(result.First(v => v.Id == 1).RecordedOn, Is.EqualTo(new DateOnly(2026, 1, 1)));
        }

        [Test]
        public async Task Should_Map_Video_Type_Title_And_Leave_It_Null_When_Untyped()
        {
            var result = (await _videoLibraryService.GetVideosAsync(ListArgs())).ToList();

            var typed = result.First(v => v.Id == 1);
            var untyped = result.First(v => v.Id == 2);

            Assert.That(typed.VideoTypeTitle, Is.EqualTo("All-Hands meetings"));
            Assert.That(untyped.VideoTypeId, Is.Null);
            Assert.That(untyped.VideoTypeTitle, Is.Null);
        }

        [Test]
        public async Task Should_Return_Only_The_Requested_Page()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(pageSize: 1));

            Assert.That(page.Count, Is.EqualTo(1));
            Assert.That(page.First().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Report_The_Whole_Library_Size_Alongside_A_Page()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(pageSize: 1));

            Assert.That(page.TotalItemCount, Is.EqualTo(2));
            Assert.That(page.PageCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Walk_Through_Pages_Without_Repeating_A_Video()
        {
            var first = await _videoLibraryService.GetVideosAsync(ListArgs(page: 1, pageSize: 1));
            var second = await _videoLibraryService.GetVideosAsync(ListArgs(page: 2, pageSize: 1));

            Assert.That(first.Single().Id, Is.EqualTo(2));
            Assert.That(second.Single().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Return_Nothing_Past_The_Last_Page()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(page: 99));

            Assert.That(page, Is.Empty);
            Assert.That(page.TotalItemCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Filter_The_Page_By_Video_Type()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(videoTypeId: 1));

            Assert.That(page.Single().Id, Is.EqualTo(1));
            Assert.That(page.TotalItemCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Filter_The_Page_Down_To_Uncategorised_Videos()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(uncategorised: true));

            Assert.That(page.Single().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Search_Across_Title_Description_And_Type()
        {
            var byTitle = await _videoLibraryService.GetVideosAsync(ListArgs(search: "Older"));
            var byDescription = await _videoLibraryService.GetVideosAsync(ListArgs(search: "Description"));
            var byType = await _videoLibraryService.GetVideosAsync(ListArgs(search: "All-Hands"));

            Assert.That(byTitle.Single().Id, Is.EqualTo(1));
            Assert.That(byDescription.Single().Id, Is.EqualTo(2));
            Assert.That(byType.Single().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Ignore_A_Blank_Search()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(search: "   "));

            Assert.That(page.TotalItemCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Not_Leak_Another_Organizations_Videos_Through_Search()
        {
            var page = await _videoLibraryService.GetVideosAsync(ListArgs(search: "another organization"));

            Assert.That(page, Is.Empty);
        }

        [Test]
        public async Task Should_Narrow_To_Videos_Added_In_The_Last_Two_Weeks()
        {
            _videos.First(v => v.Id == 1).Created = Now.AddDays(-13);
            _videos.First(v => v.Id == 2).Created = Now.AddDays(-15);

            var result = await _videoLibraryService.GetVideosAsync(ListArgs(recent: true));

            Assert.That(result.Select(v => v.Id), Is.EqualTo(new[] { 1 }));
            Assert.That(result.TotalItemCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Order_Recently_Added_Videos_By_When_They_Were_Added()
        {
            var old = _videos.First(v => v.Id == 1);
            old.RecordedOn = new DateOnly(2020, 1, 1);
            old.Created = Now.AddHours(-1);
            _videos.First(v => v.Id == 2).Created = Now.AddDays(-1);

            var result = await _videoLibraryService.GetVideosAsync(ListArgs(recent: true));

            Assert.That(result.Select(v => v.Id), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task Should_Not_Show_Another_Organizations_Recently_Added_Videos()
        {
            _videos.First(v => v.Id == 3).Created = Now.AddDays(-1);

            var result = await _videoLibraryService.GetVideosAsync(ListArgs(recent: true));

            Assert.That(result.Select(v => v.Id), Does.Not.Contain(3));
        }

        [Test]
        public async Task Should_Count_Recently_Added_Videos_In_Filters()
        {
            _videos.First(v => v.Id == 1).Created = Now.AddDays(-1);
            _videos.First(v => v.Id == 2).Created = Now.AddDays(-15);
            _videos.First(v => v.Id == 3).Created = Now.AddDays(-1);

            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs());

            Assert.That(filters.RecentCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Count_Filters_Across_The_Whole_Library_Not_One_Page()
        {
            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs());

            Assert.That(filters.TotalCount, Is.EqualTo(2));
            Assert.That(filters.UncategorisedCount, Is.EqualTo(1));
            Assert.That(filters.Types.Single().Title, Is.EqualTo("All-Hands meetings"));
            Assert.That(filters.Types.Single().VideoCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Narrow_Filter_Counts_To_The_Search()
        {
            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs(search: "Older"));

            Assert.That(filters.TotalCount, Is.EqualTo(1));
            Assert.That(filters.Types.Single().VideoCount, Is.EqualTo(1));
            Assert.That(filters.UncategorisedCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Narrow_Filter_Counts_To_The_Recording_Period()
        {
            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs(from: new DateOnly(2026, 2, 1)));

            Assert.That(filters.TotalCount, Is.EqualTo(1));
            Assert.That(filters.UncategorisedCount, Is.EqualTo(1));
            Assert.That(filters.Types.Single().VideoCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Keep_Offering_A_Type_The_Filters_Leave_Empty()
        {
            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs(to: new DateOnly(2025, 12, 31)));

            Assert.That(filters.Types.Single().Title, Is.EqualTo("All-Hands meetings"));
            Assert.That(filters.Types.Single().VideoCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Say_Whether_The_Library_Has_Uncategorised_And_Recent_Videos_Whatever_The_Filters()
        {
            _videos.First(v => v.Id == 2).Created = Now.AddDays(-1);

            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs(search: "Older"));

            Assert.That(filters.UncategorisedCount, Is.EqualTo(0));
            Assert.That(filters.HasUncategorised, Is.True);
            Assert.That(filters.RecentCount, Is.EqualTo(0));
            Assert.That(filters.HasRecent, Is.True);
        }

        [Test]
        public async Task Should_Say_When_The_Library_Has_No_Uncategorised_Or_Recent_Videos()
        {
            _videos.First(v => v.Id == 2).VideoTypeId = 1;

            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs());

            Assert.That(filters.HasUncategorised, Is.False);
            Assert.That(filters.HasRecent, Is.False);
        }

        [Test]
        public async Task Should_Not_Offer_A_Filter_For_Another_Organizations_Type()
        {
            var filters = await _videoLibraryService.GetFiltersAsync(FiltersArgs());

            Assert.That(filters.Types.Select(t => t.Title), Does.Not.Contain("Foreign type"));
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
        public async Task Should_Create_Video_With_The_Recording_Date()
        {
            var dto = NewVideoDto();
            dto.RecordedOn = new DateOnly(2024, 3, 12);

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Is<VideoLibraryItem>(v => v.RecordedOn == new DateOnly(2024, 3, 12)));
        }

        [Test]
        public async Task Should_Default_Recording_Date_To_Today_When_Not_Given()
        {
            var dto = NewVideoDto();
            dto.RecordedOn = null;

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Is<VideoLibraryItem>(v => v.RecordedOn == Today));
        }

        [Test]
        public void Should_Not_Create_Video_Recorded_In_The_Future()
        {
            var dto = NewVideoDto();
            dto.RecordedOn = Today.AddDays(2);

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.CreateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.VideoRecordedInFuture));
            _videosDbSet.DidNotReceive().Add(Arg.Any<VideoLibraryItem>());
        }

        [Test]
        public async Task Should_Accept_Tomorrow_As_Recording_Date_For_Viewers_Ahead_Of_Utc()
        {
            var dto = NewVideoDto();
            dto.RecordedOn = Today.AddDays(1);

            await _videoLibraryService.CreateVideoAsync(dto);

            _videosDbSet.Received(1).Add(Arg.Any<VideoLibraryItem>());
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

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.VideoLinkInvalid));
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
        public async Task Should_Update_Recording_Date()
        {
            var dto = NewVideoDto();
            dto.Id = 1;
            dto.RecordedOn = new DateOnly(2023, 9, 1);

            await _videoLibraryService.UpdateVideoAsync(dto);

            Assert.That(_videos.First(v => v.Id == 1).RecordedOn, Is.EqualTo(new DateOnly(2023, 9, 1)));
        }

        [Test]
        public async Task Should_Keep_Recording_Date_When_The_Update_Does_Not_Send_One()
        {
            var dto = NewVideoDto();
            dto.Id = 1;
            dto.RecordedOn = null;

            await _videoLibraryService.UpdateVideoAsync(dto);

            Assert.That(_videos.First(v => v.Id == 1).RecordedOn, Is.EqualTo(new DateOnly(2026, 1, 1)));
        }

        [Test]
        public void Should_Not_Update_Video_To_A_Future_Recording_Date()
        {
            var dto = NewVideoDto();
            dto.Id = 1;
            dto.RecordedOn = Today.AddDays(2);

            var exception = Assert.ThrowsAsync<ValidationException>(async () =>
                await _videoLibraryService.UpdateVideoAsync(dto));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.VideoRecordedInFuture));
            Assert.That(_videos.First(v => v.Id == 1).RecordedOn, Is.EqualTo(new DateOnly(2026, 1, 1)));
        }

        [Test]
        public async Task Should_Delete_The_Thumbnail_A_Video_No_Longer_Uses()
        {
            var dto = NewVideoDto();
            dto.Id = 2;
            dto.PictureId = "replacement.jpg";

            await _videoLibraryService.UpdateVideoAsync(dto);

            await _pictureService.Received(1).RemoveImageAsync("picture.jpg", TestConstants.DefaultOrganizationId);
        }

        [Test]
        public async Task Should_Delete_The_Thumbnail_When_It_Is_Cleared_Outright()
        {
            var dto = NewVideoDto();
            dto.Id = 2;
            dto.PictureId = null;

            await _videoLibraryService.UpdateVideoAsync(dto);

            await _pictureService.Received(1).RemoveImageAsync("picture.jpg", TestConstants.DefaultOrganizationId);
        }

        [Test]
        public async Task Should_Keep_The_Thumbnail_When_The_Update_Leaves_It_Alone()
        {
            var dto = NewVideoDto();
            dto.Id = 2;
            dto.PictureId = "picture.jpg";
            dto.Title = "Only the title changed";

            await _videoLibraryService.UpdateVideoAsync(dto);

            await _pictureService.DidNotReceive().RemoveImageAsync(Arg.Any<string>(), Arg.Any<int>());
        }

        [Test]
        public async Task Should_Not_Try_To_Delete_A_Thumbnail_That_Never_Existed()
        {
            var dto = NewVideoDto();
            dto.Id = 1;
            dto.PictureId = "first-picture.jpg";

            await _videoLibraryService.UpdateVideoAsync(dto);

            await _pictureService.DidNotReceive().RemoveImageAsync(Arg.Any<string>(), Arg.Any<int>());
        }

        [Test]
        public async Task Should_Keep_The_Thumbnail_Of_A_Soft_Deleted_Video()
        {
            await _videoLibraryService.RemoveVideoAsync(2, UserOrg());

            await _pictureService.DidNotReceive().RemoveImageAsync(Arg.Any<string>(), Arg.Any<int>());
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

        private static VideoLibraryListArgsDto ListArgs(
            string search = null,
            int? videoTypeId = null,
            bool uncategorised = false,
            int page = 1,
            int pageSize = 24,
            bool recent = false,
            DateOnly? from = null,
            DateOnly? to = null)
        {
            return new VideoLibraryListArgsDto
            {
                Search = search,
                VideoTypeId = videoTypeId,
                Uncategorised = uncategorised,
                Recent = recent,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize,
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
        }

        private static VideoLibraryFiltersArgsDto FiltersArgs(
            string search = null,
            DateOnly? from = null,
            DateOnly? to = null)
        {
            return new VideoLibraryFiltersArgsDto
            {
                Search = search,
                From = from,
                To = to,
                UserId = "testUser",
                OrganizationId = TestConstants.DefaultOrganizationId
            };
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
