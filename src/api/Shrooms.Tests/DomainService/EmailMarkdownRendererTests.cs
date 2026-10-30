using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
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

        private EmailMarkdownRenderer _sut;

        [SetUp]
        public void SetUp()
        {
            _markdownConverter = Substitute.For<IMarkdownConverter>();
            _mentionLinkExpander = Substitute.For<IMentionLinkExpander>();
            _customEmojiService = Substitute.For<ICustomEmojiService>();
            _appSettings = Substitute.For<IApplicationSettings>();

            _appSettings.ApiUrl.Returns("https://api.example.com/");
            _customEmojiService.GetAllAsync(3, "org").Returns(new CustomEmojiListDto
            {
                Emojis = new List<CustomEmojiDto> { new CustomEmojiDto { Name = "parrot", Url = "/storage/org/parrot.gif" } }
            });
            _mentionLinkExpander.ExpandToMarkdown(Arg.Any<string>(), Arg.Any<string>()).Returns(call => call.ArgAt<string>(0) + " [mentions]");
            _markdownConverter.ConvertToHtml(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>()).Returns("<p>html</p>");

            _sut = new EmailMarkdownRenderer(_markdownConverter, _mentionLinkExpander, _customEmojiService, _appSettings);
        }

        [Test]
        public async Task RenderAsync_ExpandsMentionsAndPassesAbsoluteEmojiUrls()
        {
            var result = await _sut.RenderAsync("hi :parrot:", 3, "org");

            Assert.That(result, Is.EqualTo("<p>html</p>"));
            _markdownConverter.Received(1).ConvertToHtml(
                "hi :parrot: [mentions]",
                Arg.Is<IReadOnlyDictionary<string, string>>(images =>
                    images.Count == 1 && images["parrot"] == "https://api.example.com/storage/org/parrot.gif"));
        }

        [Test]
        public async Task RenderAsync_OrganisationWithoutEmoji_PassesAnEmptyMap()
        {
            _customEmojiService.GetAllAsync(3, "org").Returns(new CustomEmojiListDto { Emojis = null });

            await _sut.RenderAsync("hi :parrot:", 3, "org");

            _markdownConverter.Received(1).ConvertToHtml("hi :parrot: [mentions]", Arg.Is<IReadOnlyDictionary<string, string>>(images => images.Count == 0));
        }

        [Test]
        public async Task RenderAsync_NoColon_SkipsTheEmojiLookup()
        {
            await _sut.RenderAsync("hi", 3, "org");

            await _customEmojiService.DidNotReceiveWithAnyArgs().GetAllAsync(0, null);
            _markdownConverter.Received(1).ConvertToHtml("hi [mentions]", Arg.Is<IReadOnlyDictionary<string, string>>(images => images.Count == 0));
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
