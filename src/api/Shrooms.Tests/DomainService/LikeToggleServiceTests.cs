using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Services.Wall.Likes;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class LikeToggleServiceTests
    {
        private const int OrganizationId = 2;

        private static readonly UserAndOrganizationDto UserOrg = new() { UserId = "user1", OrganizationId = OrganizationId };

        private DbSet<CustomEmoji> _customEmojisDbSet;
        private LikeToggleService _service;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _customEmojisDbSet = uow.MockDbSetForAsync<CustomEmoji>();
            SetCustomEmojis(new CustomEmoji { Name = "party-parrot", OrganizationId = OrganizationId });

            _service = new LikeToggleService(uow);
        }

        [Test]
        public async Task Should_Add_Fixed_Type_Reaction_Without_Emoji_Name()
        {
            var likes = new LikesCollection();

            await _service.ToggleAsync(likes, new AddLikeDto { Type = LikeTypeEnum.Love }, UserOrg);

            Assert.That(likes.Single().Type, Is.EqualTo(LikeTypeEnum.Love));
            Assert.That(likes.Single().EmojiName, Is.Null);
        }

        [Test]
        public async Task Should_Add_Custom_Emoji_Reaction()
        {
            var likes = new LikesCollection();

            await _service.ToggleAsync(likes, EmojiReaction("party-parrot"), UserOrg);

            Assert.That(likes.Single().UserId, Is.EqualTo("user1"));
            Assert.That(likes.Single().Type, Is.EqualTo(LikeTypeEnum.Emoji));
            Assert.That(likes.Single().EmojiName, Is.EqualTo("party-parrot"));
        }

        [Test]
        public async Task Should_Add_Standard_Emoji_Reaction()
        {
            var likes = new LikesCollection();

            await _service.ToggleAsync(likes, EmojiReaction("tada"), UserOrg);

            Assert.That(likes.Single().EmojiName, Is.EqualTo("tada"));
        }

        [Test]
        public async Task Should_Keep_First_Emoji_Reaction_When_Same_User_Adds_Another_Emoji()
        {
            var likes = new LikesCollection { new Like("user1", LikeTypeEnum.Emoji, "tada") };

            await _service.ToggleAsync(likes, EmojiReaction("party-parrot"), UserOrg);

            Assert.That(likes.Select(like => like.EmojiName), Is.EquivalentTo(new[] { "tada", "party-parrot" }));
        }

        [Test]
        public async Task Should_Remove_Only_The_Matching_Emoji_Reaction()
        {
            var likes = new LikesCollection
            {
                new Like("user1", LikeTypeEnum.Emoji, "tada"),
                new Like("user1", LikeTypeEnum.Emoji, "party-parrot"),
                new Like("user2", LikeTypeEnum.Emoji, "party-parrot")
            };

            await _service.ToggleAsync(likes, EmojiReaction("party-parrot"), UserOrg);

            Assert.That(likes.Count, Is.EqualTo(2));
            Assert.That(likes.Any(like => like.UserId == "user1" && like.EmojiName == "party-parrot"), Is.False);
        }

        [Test]
        public async Task Should_Remove_Reaction_Whose_Custom_Emoji_Was_Deleted()
        {
            var likes = new LikesCollection { new Like("user1", LikeTypeEnum.Emoji, "deleted-emoji") };

            await _service.ToggleAsync(likes, EmojiReaction("deleted-emoji"), UserOrg);

            Assert.That(likes, Is.Empty);
        }

        [Test]
        public void Should_Reject_Emoji_That_Is_Neither_Custom_Nor_Standard()
        {
            var likes = new LikesCollection();

            var exception = Assert.ThrowsAsync<ValidationException>(() => _service.ToggleAsync(likes, EmojiReaction("no-such-emoji"), UserOrg));

            Assert.That(exception.ErrorCode, Is.EqualTo(ErrorCodes.ReactionEmojiInvalid));
            Assert.That(likes, Is.Empty);
        }

        [Test]
        public void Should_Reject_Custom_Emoji_Of_Another_Organization()
        {
            SetCustomEmojis(new CustomEmoji { Name = "party-parrot", OrganizationId = OrganizationId + 1 });

            Assert.ThrowsAsync<ValidationException>(() => _service.ToggleAsync(new LikesCollection(), EmojiReaction("party-parrot"), UserOrg));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Party Parrot")]
        [TestCase("<script>")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void Should_Reject_Emoji_Reaction_With_Missing_Or_Malformed_Name(string emojiName)
        {
            Assert.ThrowsAsync<ValidationException>(() => _service.ToggleAsync(new LikesCollection(), EmojiReaction(emojiName), UserOrg));
        }

        [Test]
        public void Should_Reject_Emoji_Name_On_Fixed_Type_Reaction()
        {
            var addLike = new AddLikeDto { Type = LikeTypeEnum.Like, EmojiName = "tada" };

            Assert.ThrowsAsync<ValidationException>(() => _service.ToggleAsync(new LikesCollection(), addLike, UserOrg));
        }

        private static AddLikeDto EmojiReaction(string emojiName)
        {
            return new AddLikeDto { Type = LikeTypeEnum.Emoji, EmojiName = emojiName };
        }

        private void SetCustomEmojis(params CustomEmoji[] emojis)
        {
            _customEmojisDbSet.SetDbSetDataForAsync(emojis.AsQueryable());
        }
    }
}
