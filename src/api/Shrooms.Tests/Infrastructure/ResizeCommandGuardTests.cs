using System.Linq;
using NUnit.Framework;
using Shrooms.Presentation.Api.Middlewares;
using SixLabors.ImageSharp.Web.Commands;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class ResizeCommandGuardTests
    {
        [Test]
        public void Clamp_LeavesAllowedSizesUntouched()
        {
            var commands = new CommandCollection { { "width", "640" }, { "height", "480" }, { "rmode", "max" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["width"], Is.EqualTo("640"));
            Assert.That(commands["height"], Is.EqualTo("480"));
            Assert.That(commands["rmode"], Is.EqualTo("max"));
        }

        [TestCase(1, 32)]
        [TestCase(100, 128)]
        [TestCase(129, 192)]
        [TestCase(500, 640)]
        [TestCase(2048, 2048)]
        [TestCase(2049, 2048)]
        [TestCase(99999, 2048)]
        public void Clamp_SnapsRequestedSizesUpToTheNextAllowedSize(int requested, int expected)
        {
            var commands = new CommandCollection { { "width", requested.ToString() }, { "height", requested.ToString() } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["width"], Is.EqualTo(expected.ToString()));
            Assert.That(commands["height"], Is.EqualTo(expected.ToString()));
        }

        [TestCase("0128", "128")]
        [TestCase("+128", "128")]
        [TestCase("00640", "640")]
        public void Clamp_CanonicalisesEquivalentSpellings_SoTheyShareOneCacheEntry(string raw, string expected)
        {
            var commands = new CommandCollection { { "width", raw } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["width"], Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("-5")]
        [TestCase("0")]
        [TestCase("")]
        public void Clamp_DropsInvalidDimensions(string raw)
        {
            var commands = new CommandCollection { { "width", raw }, { "height", "128" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Contains("width"), Is.False);
            Assert.That(commands["height"], Is.EqualTo("128"));
        }

        [TestCase("rmode", "crop")]
        [TestCase("rsampler", "bicubic")]
        [TestCase("ranchor", "top")]
        public void Clamp_EmptiesRequestsWithoutDimensions_SoNothingIsProcessedOrCached(string key, string value)
        {
            var commands = new CommandCollection { { key, value } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Count, Is.EqualTo(0));
        }

        [Test]
        public void Clamp_EmptiesRequestsWhoseOnlyDimensionIsInvalid()
        {
            var commands = new CommandCollection { { "width", "abc" }, { "rmode", "max" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Count, Is.EqualTo(0));
        }

        [TestCase("max", "max")]
        [TestCase("MAX", "max")]
        [TestCase(" Crop ", "crop")]
        [TestCase("pad", "pad")]
        public void Clamp_CanonicalisesTheResizeMode(string raw, string expected)
        {
            var commands = new CommandCollection { { "width", "128" }, { "rmode", raw } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["rmode"], Is.EqualTo(expected));
        }

        [TestCase("foo")]
        [TestCase("")]
        [TestCase("max;drop")]
        [TestCase("min")]
        public void Clamp_DropsUnknownResizeModes(string raw)
        {
            var commands = new CommandCollection { { "width", "128" }, { "rmode", raw } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Contains("rmode"), Is.False);
            Assert.That(commands["width"], Is.EqualTo("128"));
        }

        [Test]
        public void Clamp_DropsEveryCommandOtherThanWidthHeightAndMode()
        {
            var commands = new CommandCollection
            {
                { "width", "128" }, { "height", "128" }, { "rmode", "crop" },
                { "rsampler", "aaa" }, { "ranchor", "zz" }, { "rxy", "1,2" }, { "compand", "xx" }, { "orient", "yy" }, { "mode", "max" },
            };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Select(c => c.Key).OrderBy(k => k), Is.EqualTo(new[] { "height", "rmode", "width" }));
        }

        [TestCase(2000, 20000, 2048, null, 2048, 2048, Description = "tall source, width only: implied height 20480")]
        [TestCase(20000, 2000, null, 2048, 2048, 2048, Description = "wide source, height only: implied width 20480")]
        [TestCase(1000, 5000, 640, null, 640, 2048, Description = "feed width on a tall post image: implied height 3200")]
        public void BoundImpliedDimension_FitsIntoTheCapWhenTheOtherSideWouldExceedIt(int srcW, int srcH, int? reqW, int? reqH, int expectedW, int expectedH)
        {
            var box = ClampingResizeWebProcessor.BoundImpliedDimension(srcW, srcH, reqW, reqH, ResizeCommandGuard.MaxDimension);

            Assert.That(box, Is.EqualTo((expectedW, expectedH)));
        }

        [TestCase(4000, 3000, 640, null, Description = "implied height 480 is fine")]
        [TestCase(4000, 3000, 640, 480, Description = "both given: already snapped by the guard")]
        [TestCase(4000, 3000, null, null)]
        [TestCase(0, 3000, 640, null)]
        public void BoundImpliedDimension_LeavesOrdinaryRequestsAlone(int srcW, int srcH, int? reqW, int? reqH)
        {
            Assert.That(ClampingResizeWebProcessor.BoundImpliedDimension(srcW, srcH, reqW, reqH, ResizeCommandGuard.MaxDimension), Is.Null);
        }
    }
}
