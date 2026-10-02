using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;
using Shrooms.Infrastructure.Storage;
using Shrooms.Tests.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PictureServiceTests
    {
        private IPictureService _pictureService;
        private IStorage _storage;
        private IPictureReferenceService _references;
        private DbSet<Organization> _organizationsDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _organizationsDbSet = uow.MockDbSetForAsync<Organization>();
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new() { Id = 2, ShortName = "pictures" }
            }.AsQueryable());

            _storage = Substitute.For<IStorage>();
            _references = Substitute.For<IPictureReferenceService>();
            _references.CountReferencesAsync(Arg.Any<string>()).Returns(Task.FromResult(0));
            _pictureService = new PictureService(_storage, uow, _references);
        }

        // Real, tiny encoded images: the service now reads the header with ImageSharp, so magic bytes alone
        // are not enough.
        private static MemoryStream TinyImage(string format, int width = 2, int height = 2)
        {
            using var image = new Image<Rgba32>(width, height);
            var stream = new MemoryStream();
            switch (format)
            {
                case "jpg": image.SaveAsJpeg(stream); break;
                case "png": image.SaveAsPng(stream); break;
                case "gif": image.SaveAsGif(stream); break;
                case "bmp": image.SaveAsBmp(stream); break;
                case "webp": image.SaveAsWebp(stream); break;
            }

            stream.Position = 0;
            return stream;
        }

        [TestCase("jpg", "photo.jpg", "image/png", ".jpg")]
        [TestCase("jpg", "photo.JPG", null, ".jpg")]
        [TestCase("webp", "photo.webp", null, ".webp")]
        [TestCase("png", "blob", "image/png", ".png")]
        [TestCase("jpg", "photo.html", "image/jpeg", ".jpg")]
        [TestCase("gif", "photo.svg", "image/gif", ".gif")]
        [TestCase("bmp", "photo.p n g", "image/bmp", ".bmp")]
        [TestCase("png", "photo.jpg", null, ".png")]
        [TestCase("gif", "photo.png", "image/jpeg", ".gif")]
        public async Task UploadFromStream_ShouldStoreOnlyAllowlistedImageExtensions(string format, string fileName, string mimeType, string expectedExtension)
        {
            using var stream = TinyImage(format);

            var result = await _pictureService.UploadFromStreamAsync(stream, mimeType, fileName, 2);

            Assert.That(result, Does.EndWith(expectedExtension));
            Assert.That(BlobKeyGuard.IsSafeBlobKey(result), Is.True);
            Assert.That(BlobKeyGuard.HasAllowedImageExtension(result), Is.True);
            Assert.That(stream.Position, Is.EqualTo(0), "the stream must be rewound for the storage upload");
        }

        [TestCase("photo.html", "text/html")]
        [TestCase("photo.svg", "image/svg+xml")]
        [TestCase("photo.exe", null)]
        [TestCase("noextension", "application/octet-stream")]
        public void UploadFromStream_ShouldReject_WhenNeitherExtensionNorMimeTypeIsAnAllowedImage(string fileName, string mimeType)
        {
            using var stream = TinyImage("png");

            Assert.That(
                async () => await _pictureService.UploadFromStreamAsync(stream, mimeType, fileName, 2),
                Throws.ArgumentException);

            _storage.DidNotReceiveWithAnyArgs().UploadPictureAsync(default, default, default, default);
        }

        [Test]
        public void UploadFromStream_ShouldReject_WhenBytesAreNotAnImage()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("<html><script>alert(1)</script></html>"));

            Assert.That(
                async () => await _pictureService.UploadFromStreamAsync(stream, "image/png", "photo.png", 2),
                Throws.ArgumentException);
        }

        [TestCase("jpg", "image/png", "anything.png", ".jpg")]
        [TestCase("png", "image/jpeg", "x.jpg", ".png")]
        [TestCase("gif", "image/png", "polyglot.html", ".gif")]
        [TestCase("webp", "image/jpeg", "clip.exe", ".webp")]
        public async Task UploadOriginal_ShouldUseDetectedFormat_NotFileNameOrMimeType(string format, string mimeType, string fileName, string expectedExtension)
        {
            using var stream = TinyImage(format);

            var result = await _pictureService.UploadOriginalAsync(stream, mimeType, fileName, 2);

            Assert.That(result, Does.EndWith(expectedExtension));
            await _storage.Received(1).UploadPictureAsync(Arg.Any<Stream>(), result, mimeType, "pictures");
        }

        [Test]
        public void UploadOriginal_ShouldThrow_WhenStreamIsNotARecognizedImage()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            Assert.That(
                async () => await _pictureService.UploadOriginalAsync(stream, "image/jpeg", "garbage.jpg", 2),
                Throws.ArgumentException);
        }

        [Test]
        public void UploadOriginal_ShouldThrow_WhenAGifPolyglotCarriesScriptInsteadOfImageData()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("GIF89a<script>alert(1)</script>"));

            Assert.That(
                async () => await _pictureService.UploadOriginalAsync(stream, "image/png", "polyglot.html", 2),
                Throws.ArgumentException);
            _storage.DidNotReceiveWithAnyArgs().UploadPictureAsync(default, default, default, default);
        }

        [Test]
        public void Upload_ShouldReject_WhenDeclaredCanvasExceedsPixelLimit()
        {
            // A 7000x7000 PNG is 49 MP, over the 40 MP limit; the file itself is small because it is blank.
            using var stream = TinyImage("png", 7000, 7000);

            Assert.That(
                async () => await _pictureService.UploadOriginalAsync(stream, "image/png", "huge.png", 2),
                Throws.ArgumentException.With.Message.Contains("too large"));
            stream.Position = 0;
            Assert.That(
                async () => await _pictureService.UploadFromStreamAsync(stream, "image/png", "huge.png", 2),
                Throws.ArgumentException.With.Message.Contains("too large"));
        }

        [Test]
        public async Task RemoveImage_ShouldForwardToStorage_WhenKeyIsBareFileName()
        {
            await _pictureService.RemoveImageAsync("746afdc0-ea3b-4a94-836e-5e3e7774bca3.jpg", 2);

            await _storage.Received(1).RemovePictureAsync("746afdc0-ea3b-4a94-836e-5e3e7774bca3.jpg", "pictures");
        }

        [Test]
        public async Task RemoveImage_ShouldNotDelete_WhenAnotherRecordStillUsesThePicture()
        {
            _references.CountReferencesAsync("victim.jpg").Returns(Task.FromResult(1));

            await _pictureService.RemoveImageAsync("victim.jpg", 2);

            await _storage.DidNotReceiveWithAnyArgs().RemovePictureAsync(default, default);
        }

        [TestCase("../../appsettings.json")]
        [TestCase("..\\..\\appsettings.json")]
        [TestCase("sub/dir.jpg")]
        [TestCase("")]
        [TestCase(null)]
        public async Task RemoveImage_ShouldNotTouchStorage_WhenKeyIsNotABareFileName(string key)
        {
            await _pictureService.RemoveImageAsync(key, 2);

            await _storage.DidNotReceiveWithAnyArgs().RemovePictureAsync(default, default);
        }
    }
}
