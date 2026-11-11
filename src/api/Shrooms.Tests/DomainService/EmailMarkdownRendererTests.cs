using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Email;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class EmailMarkdownRendererTests
    {
        private IMarkdownConverter _markdownConverter;
        private IMentionLinkExpander _mentionLinkExpander;
        private ICustomEmojiService _customEmojiService;
        private IApplicationSettings _appSettings;
        private ILogger<EmailMarkdownRenderer> _logger;

        private EmailMarkdownRenderer _sut;

        [SetUp]
        public void SetUp()
        {
            _markdownConverter = Substitute.For<IMarkdownConverter>();
            _mentionLinkExpander = Substitute.For<IMentionLinkExpander>();
            _customEmojiService = Substitute.For<ICustomEmojiService>();
            _appSettings = Substitute.For<IApplicationSettings>();
            _logger = Substitute.For<ILogger<EmailMarkdownRenderer>>();

            _appSettings.ApiUrl.Returns("https://api.example.com/");
            _customEmojiService.GetAllAsync(3, "org", Arg.Any<CancellationToken>()).Returns(new CustomEmojiListDto
            {
                Emojis = new List<CustomEmojiDto> { new CustomEmojiDto { Name = "parrot", Url = "/storage/org/parrot.gif" } }
            });
            _mentionLinkExpander.ExpandToMarkdown(Arg.Any<string>(), Arg.Any<string>()).Returns(call => call.ArgAt<string>(0) + " [mentions]");
            _markdownConverter.ConvertToHtml(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<int?>()).Returns("<p>html</p>");

            _sut = new EmailMarkdownRenderer(_markdownConverter, _mentionLinkExpander, _customEmojiService, _appSettings, _logger);
        }

        [Test]
        public async Task RenderAsync_ExpandsMentionsAndPassesAbsoluteEmojiUrls()
        {
            var result = await _sut.RenderAsync("hi :parrot:", 3, "org");

            Assert.That(result, Is.EqualTo("<p>html</p>"));
            _markdownConverter.Received(1).ConvertToHtml(
                "hi :parrot: [mentions]",
                Arg.Is<IReadOnlyDictionary<string, string>>(images =>
                    images.Count == 1 && images["parrot"] == "https://api.example.com/storage/org/parrot.gif"),
                null);
        }

        [Test]
        public async Task RenderAsync_WithMaxTextLength_PassesItToTheConverter()
        {
            await _sut.RenderAsync("hi :parrot:", 3, "org", 150);

            _markdownConverter.Received(1).ConvertToHtml("hi :parrot: [mentions]", Arg.Any<IReadOnlyDictionary<string, string>>(), 150);
        }

        [Test]
        public async Task RenderAsync_OrganisationWithoutEmoji_PassesAnEmptyMap()
        {
            _customEmojiService.GetAllAsync(3, "org", Arg.Any<CancellationToken>()).Returns(new CustomEmojiListDto { Emojis = null });

            await _sut.RenderAsync("hi :parrot:", 3, "org");

            _markdownConverter.Received(1).ConvertToHtml("hi :parrot: [mentions]", Arg.Is<IReadOnlyDictionary<string, string>>(images => images.Count == 0), null);
        }

        [Test]
        public async Task RenderAsync_EmojiLookupFails_RendersWithoutCustomEmoji()
        {
            _customEmojiService.GetAllAsync(3, "org", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("db down"));

            var result = await _sut.RenderAsync("hi :parrot:", 3, "org");

            Assert.That(result, Is.EqualTo("<p>html</p>"));
            _markdownConverter.Received(1).ConvertToHtml("hi :parrot: [mentions]", Arg.Is<IReadOnlyDictionary<string, string>>(images => images.Count == 0), null);
        }

        [Test]
        public async Task RenderAsync_EmojiLookupFails_LogsTheError()
        {
            var failure = new InvalidOperationException("db down");
            _customEmojiService.GetAllAsync(3, "org", Arg.Any<CancellationToken>()).ThrowsAsync(failure);

            await _sut.RenderAsync("hi :parrot:", 3, "org");

            var errors = _logger.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
                .Select(call => call.GetArguments())
                .Where(args => (LogLevel)args[0] == LogLevel.Error)
                .ToList();
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0][3], Is.SameAs(failure));
        }

        [TestCase("hi")]
        [TestCase("see https://x.y at 10:30")]
        public async Task RenderAsync_NoShortcode_SkipsTheEmojiLookup(string markdown)
        {
            await _sut.RenderAsync(markdown, 3, "org");

            await _customEmojiService.DidNotReceiveWithAnyArgs().GetAllAsync(0, null);
            _markdownConverter.Received(1).ConvertToHtml(markdown + " [mentions]", Arg.Is<IReadOnlyDictionary<string, string>>(images => images.Count == 0), null);
        }

        [Test]
        public async Task RenderAsync_EmptyMarkdown_ReturnsEmptyWithoutLookingAnythingUp()
        {
            var result = await _sut.RenderAsync(string.Empty, 3, "org");

            Assert.That(result, Is.Empty);
            await _customEmojiService.DidNotReceiveWithAnyArgs().GetAllAsync(0, null);
            _markdownConverter.DidNotReceiveWithAnyArgs().ConvertToHtml(null, null);
        }
    }
}
