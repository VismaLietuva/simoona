using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class EmojiControllerTests
    {
        private EmojiController _emojiController;

        private ICustomEmojiService _customEmojiService;

        [SetUp]
        public void TestInitializer()
        {
            _customEmojiService = Substitute.For<ICustomEmojiService>();

            _emojiController = new EmojiController(ModelMapper.Create(), _customEmojiService);
            _emojiController.SetUpControllerForTesting();
        }

        private static byte[] CreatePngBytes(int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        private static IFormFile CreateFormFile(byte[] content, string contentType, string fileName = "emoji.png", long? lengthOverride = null)
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, lengthOverride ?? stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Test]
        public async Task List_ReturnsOk()
        {
            _customEmojiService
                .GetAllAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(new CustomEmojiListDto { Emojis = new List<CustomEmojiDto>(), ETag = "abc123" });

            var result = await _emojiController.List();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task List_SetsETagAndCacheControlHeaders()
        {
            _customEmojiService
                .GetAllAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(new CustomEmojiListDto { Emojis = new List<CustomEmojiDto>(), ETag = "abc123" });

            await _emojiController.List();

            Assert.That(_emojiController.Response.Headers.ETag.ToString(), Is.EqualTo("\"abc123\""));
            Assert.That(_emojiController.Response.Headers.CacheControl.ToString(), Is.EqualTo("private"));
        }

        [Test]
        public async Task List_WhenIfNoneMatchMatches_Returns304()
        {
            _customEmojiService
                .GetAllAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(new CustomEmojiListDto { Emojis = new List<CustomEmojiDto>(), ETag = "abc123" });

            _emojiController.Request.Headers.IfNoneMatch = "\"abc123\"";

            var result = await _emojiController.List();

            Assert.That(result.GetStatusCode(), Is.EqualTo((HttpStatusCode)304));
        }

        [Test]
        public async Task Create_WhenNoFile_ReturnsBadRequest()
        {
            var result = await _emojiController.Create("party-parrot", null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_WhenFileTooLarge_ReturnsBadRequest()
        {
            var file = CreateFormFile(new byte[] { 1 }, "image/png", lengthOverride: WebApiConstants.MaximumCustomEmojiSizeInBytes + 1);

            var result = await _emojiController.Create("party-parrot", file);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_WhenContentTypeNotAllowed_ReturnsUnsupportedMediaType()
        {
            var file = CreateFormFile(CreatePngBytes(1, 1), "image/bmp");

            var result = await _emojiController.Create("party-parrot", file);

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
        }

        [Test]
        public async Task Create_WhenFileIsValid_ReturnsOk()
        {
            var file = CreateFormFile(CreatePngBytes(128, 128), "image/png");

            _customEmojiService
                .CreateAsync(
                    Arg.Is<NewCustomEmojiDto>(x => x.Name == "party-parrot" && x.MimeType == "image/png" && x.FileName == "emoji.png"),
                    Arg.Any<UserAndOrganizationDto>(),
                    Arg.Any<string>())
                .Returns(new CustomEmojiDto { Id = 1, Name = "party-parrot", Url = "/storage/visma/a.png", AuthorId = "1" });

            var result = await _emojiController.Create("party-parrot", file);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Create_WhenNameIsDuplicate_ReturnsConflict()
        {
            var file = CreateFormFile(CreatePngBytes(128, 128), "image/png");

            _customEmojiService
                .CreateAsync(Arg.Any<NewCustomEmojiDto>(), Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .ThrowsAsync(new ValidationException(ErrorCodes.DuplicatesIntolerable, "Emoji name already exists"));

            var result = await _emojiController.Create("party-parrot", file);

            Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Create_WhenNameIsInvalid_ReturnsBadRequest()
        {
            var file = CreateFormFile(CreatePngBytes(128, 128), "image/png");

            _customEmojiService
                .CreateAsync(Arg.Any<NewCustomEmojiDto>(), Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .ThrowsAsync(new ValidationException(ErrorCodes.InvalidCustomEmojiName, "Emoji name is invalid"));

            var result = await _emojiController.Create("PARTY", file);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Delete_WhenSuccessful_ReturnsOk()
        {
            var result = await _emojiController.Delete(1);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Delete_WhenEmojiDoesNotExist_ReturnsBadRequest()
        {
            _customEmojiService
                .DeleteAsync(1, Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new ValidationException(ErrorCodes.ContentDoesNotExist, "Emoji does not exist"));

            var result = await _emojiController.Delete(1);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Delete_WhenNotOwnerAndNotAdmin_ReturnsForbidden()
        {
            _customEmojiService
                .DeleteAsync(1, Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new UnauthorizedException());

            var result = await _emojiController.Delete(1);

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.Forbidden));
        }
    }
}
