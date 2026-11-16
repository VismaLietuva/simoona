using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.Tests.DataLayer
{
    [TestFixture]
    public class LikesCollectionTests
    {
        [Test]
        public void Should_Keep_Emoji_Name_When_Serialized_And_Read_Back()
        {
            var stored = new LikesCollection { new Like("user1", LikeTypeEnum.Emoji, "party-parrot") }.Serialized;

            var loaded = new LikesCollection { Serialized = stored };

            Assert.That(loaded.Single().UserId, Is.EqualTo("user1"));
            Assert.That(loaded.Single().Type, Is.EqualTo(LikeTypeEnum.Emoji));
            Assert.That(loaded.Single().EmojiName, Is.EqualTo("party-parrot"));
        }

        [Test]
        public void Should_Read_Likes_Stored_Before_Emoji_Name_Existed()
        {
            var loaded = new LikesCollection
            {
                Serialized = "[{\"UserId\":\"user1\",\"Created\":\"2026-01-05T10:00:00Z\",\"Type\":2}]"
            };

            Assert.That(loaded.Single().Type, Is.EqualTo(LikeTypeEnum.Lol));
            Assert.That(loaded.Single().EmojiName, Is.Null);
        }

        [Test]
        public void Should_Not_Store_Emoji_Name_For_Fixed_Type_Likes()
        {
            var stored = new LikesCollection { new Like("user1", LikeTypeEnum.Like) }.Serialized;

            Assert.That(stored, Does.Not.Contain("EmojiName"));
        }
    }
}
