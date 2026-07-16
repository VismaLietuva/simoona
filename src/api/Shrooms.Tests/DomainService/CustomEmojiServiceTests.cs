using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.ServiceValidators.Validators.Emoji;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class CustomEmojiServiceTests
    {
        private DbSet<CustomEmoji> _customEmojisDbSet;
        private DbSet<Organization> _organizationsDbSet;
        private IPictureService _pictureService;
        private IPermissionService _permissionService;
        private ICustomEmojiValidator _validator;
        private ICustomEmojiService _customEmojiService;

        private readonly UserAndOrganizationDto _userOrg = new()
        {
            UserId = "user1",
            OrganizationId = 2
        };

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _customEmojisDbSet = uow.MockDbSetForAsync<CustomEmoji>();
            _organizationsDbSet = uow.MockDbSetForAsync<Organization>();

            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new()
                    { Id = 2, ShortName = "Visma" }
            }.AsQueryable());

            _pictureService = Substitute.For<IPictureService>();
            _permissionService = Substitute.For<IPermissionService>();
            _validator = Substitute.For<ICustomEmojiValidator>();

            _customEmojiService = new CustomEmojiService(uow, _pictureService, _permissionService, _validator);
        }

        [Test]
        public async Task Should_Create_New_Custom_Emoji()
        {
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji>().AsQueryable());
            _pictureService
                .UploadFromStreamAsync(Arg.Any<Stream>(), "image/png", "parrot.png", 2)
                .Returns("blob-guid.png");

            await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var result = await _customEmojiService.CreateAsync("party-parrot", stream, "image/png", "parrot.png", _userOrg);

            _customEmojisDbSet.Received().Add(
                Arg.Is<CustomEmoji>(x =>
                    x.Name == "party-parrot" &&
                    x.BlobName == "blob-guid.png" &&
                    x.AuthorId == "user1" &&
                    x.OrganizationId == 2));

            Assert.That(result.Name, Is.EqualTo("party-parrot"));
            Assert.That(result.Url, Is.EqualTo("/storage/visma/blob-guid.png"));
            Assert.That(result.AuthorId, Is.EqualTo("user1"));
        }

        [Test]
        public async Task Should_Validate_Name_Before_Uploading()
        {
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji>().AsQueryable());
            _pictureService
                .UploadFromStreamAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns("blob-guid.png");

            await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            await _customEmojiService.CreateAsync("party-parrot", stream, "image/png", "parrot.png", _userOrg);

            Received.InOrder(() =>
            {
                _validator.CheckNameFormat("party-parrot");
                _validator.CheckIfNameIsTakenAsync("party-parrot", 2);
                _pictureService.UploadFromStreamAsync(Arg.Any<Stream>(), "image/png", "parrot.png", 2);
            });
        }

        [Test]
        public async Task Should_Return_All_Organization_Emojis_With_Urls()
        {
            var emojis = new List<CustomEmoji>
            {
                new()
                    { Id = 1, Name = "party-parrot", BlobName = "a.gif", AuthorId = "user1", OrganizationId = 2, IsDeleted = false },
                new()
                    { Id = 2, Name = "ship-it", BlobName = "b.png", AuthorId = "user2", OrganizationId = 2, IsDeleted = false },
                new()
                    { Id = 3, Name = "other-org", BlobName = "c.png", AuthorId = "user3", OrganizationId = 3, IsDeleted = false }
            };
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());

            var result = (await _customEmojiService.GetAllAsync(_userOrg)).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("party-parrot"));
            Assert.That(result[0].Url, Is.EqualTo("/storage/visma/a.gif"));
            Assert.That(result[1].Name, Is.EqualTo("ship-it"));
        }

        [Test]
        public async Task Should_Not_Return_Soft_Deleted_Emojis()
        {
            var emojis = new List<CustomEmoji>
            {
                new()
                    { Id = 1, Name = "party-parrot", BlobName = "a.gif", AuthorId = "user1", OrganizationId = 2, IsDeleted = true }
            };
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());

            var result = await _customEmojiService.GetAllAsync(_userOrg);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task Should_Delete_Own_Emoji()
        {
            var emoji = new CustomEmoji
            {
                Id = 1,
                Name = "party-parrot",
                BlobName = "a.gif",
                AuthorId = "user1",
                OrganizationId = 2,
                IsDeleted = false
            };
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji> { emoji }.AsQueryable());

            await _customEmojiService.DeleteAsync(1, _userOrg);

            _customEmojisDbSet.Received().Remove(Arg.Is<CustomEmoji>(x => x.Id == 1));
        }

        [Test]
        public async Task Should_Delete_Other_User_Emoji_When_Admin()
        {
            var emoji = new CustomEmoji
            {
                Id = 1,
                Name = "party-parrot",
                BlobName = "a.gif",
                AuthorId = "user2",
                OrganizationId = 2,
                IsDeleted = false
            };
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji> { emoji }.AsQueryable());
            _permissionService.UserHasPermissionAsync(_userOrg, AdministrationPermissions.CustomEmoji).Returns(true);

            await _customEmojiService.DeleteAsync(1, _userOrg);

            _customEmojisDbSet.Received().Remove(Arg.Is<CustomEmoji>(x => x.Id == 1));
        }

        [Test]
        public void Should_Throw_When_Deleting_Other_User_Emoji_Without_Admin_Permission()
        {
            var emoji = new CustomEmoji
            {
                Id = 1,
                Name = "party-parrot",
                BlobName = "a.gif",
                AuthorId = "user2",
                OrganizationId = 2,
                IsDeleted = false
            };
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji> { emoji }.AsQueryable());
            _permissionService.UserHasPermissionAsync(_userOrg, AdministrationPermissions.CustomEmoji).Returns(false);

            Assert.ThrowsAsync<UnauthorizedException>(async () => await _customEmojiService.DeleteAsync(1, _userOrg));
        }

        [Test]
        public void Should_Throw_When_Emoji_To_Delete_Does_Not_Exist()
        {
            _customEmojisDbSet.SetDbSetDataForAsync(new List<CustomEmoji>().AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _customEmojiService.DeleteAsync(1, _userOrg));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }
    }
}
