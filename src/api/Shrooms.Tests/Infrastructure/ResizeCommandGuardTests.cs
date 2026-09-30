using NUnit.Framework;
using Shrooms.Presentation.Api.Middlewares;
using SixLabors.ImageSharp.Web.Commands;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class ResizeCommandGuardTests
    {
        [Test]
        public void Clamp_LeavesDimensionsWithinLimitUntouched()
        {
            var commands = new CommandCollection { { "width", "640" }, { "height", "480" }, { "rmode", "max" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["width"], Is.EqualTo("640"));
            Assert.That(commands["height"], Is.EqualTo("480"));
            Assert.That(commands["rmode"], Is.EqualTo("max"));
        }

        [Test]
        public void Clamp_CapsOversizedDimensions()
        {
            var commands = new CommandCollection { { "width", "99999" }, { "height", "100000" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands["width"], Is.EqualTo(ResizeCommandGuard.MaxDimension.ToString()));
            Assert.That(commands["height"], Is.EqualTo(ResizeCommandGuard.MaxDimension.ToString()));
        }

        [TestCase("abc")]
        [TestCase("-5")]
        [TestCase("0")]
        [TestCase("")]
        public void Clamp_DropsInvalidDimensions(string raw)
        {
            var commands = new CommandCollection { { "width", raw }, { "height", "100" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Contains("width"), Is.False);
            Assert.That(commands["height"], Is.EqualTo("100"));
        }

        [Test]
        public void Clamp_IgnoresRequestsWithoutDimensions()
        {
            var commands = new CommandCollection { { "rmode", "crop" } };

            ResizeCommandGuard.Clamp(commands);

            Assert.That(commands.Count, Is.EqualTo(1));
        }
    }
}
