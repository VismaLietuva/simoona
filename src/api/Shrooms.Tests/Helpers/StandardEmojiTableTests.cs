using NUnit.Framework;
using Shrooms.Domain.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class StandardEmojiTableTests
    {
        [Test]
        public void TryGet_Tada_ReturnsThePartyPopper()
        {
            Assert.That(StandardEmojiTable.TryGet("tada", out var variants), Is.True);
            Assert.That(variants, Is.EqualTo(new[] { "🎉" }));
        }

        [Test]
        public void TryGet_ThumbsUp_ReturnsSixToneVariants()
        {
            Assert.That(StandardEmojiTable.TryGet("+1", out var variants), Is.True);
            Assert.That(variants, Has.Count.EqualTo(6));
            Assert.That(variants[0], Is.EqualTo("👍"));
        }

        [Test]
        public void TryGet_UnknownName_ReturnsFalse()
        {
            Assert.That(StandardEmojiTable.TryGet("definitely-not-an-emoji", out _), Is.False);
        }

        [Test]
        public void Count_CoversTheWholeTable()
        {
            Assert.That(StandardEmojiTable.Count, Is.GreaterThan(1800));
        }
    }
}
