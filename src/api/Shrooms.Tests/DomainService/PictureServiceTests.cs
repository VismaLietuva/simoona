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
            _references.IsReferencedAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
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

        private static MemoryStream AnimatedGif(int frames, int size = 4)
        {
            using var image = new Image<Rgba32>(size, size);
            for (var i = 1; i < frames; i++)
            {
                using var frame = new Image<Rgba32>(size, size);
                image.Frames.AddFrame(frame.Frames.RootFrame);
            }

            var stream = new MemoryStream();
            image.SaveAsGif(stream);
            stream.Position = 0;
            return stream;
        }

        [Test]
        public void GifFrameCounter_CountsFramesWithoutDecoding()
        {
            using var stream = AnimatedGif(7);

            Assert.That(GifFrameCounter.Count(stream, 1000), Is.EqualTo(7));
            Assert.That(stream.Position, Is.EqualTo(0));
            Assert.That(GifFrameCounter.Count(new MemoryStream(new byte[] { 1, 2, 3 }), 10), Is.EqualTo(-1));
        }

        [Test]
        public async Task UploadOriginal_ShouldAcceptAGifWithinTheFrameBudget()
        {
            using var stream = AnimatedGif(10);

            var result = await _pictureService.UploadOriginalAsync(stream, "image/gif", "anim.gif", 2);

            Assert.That(result, Does.EndWith(".gif"));
        }

        [Test]
        public void UploadOriginal_ShouldReject_WhenAGifHasTooManyFrames()
        {
            using var stream = AnimatedGif(PictureService.MaxGifFrames + 5);

            Assert.That(
                async () => await _pictureService.UploadOriginalAsync(stream, "image/gif", "bomb.gif", 2),
                Throws.ArgumentException.With.Message.Contains("frames"));
            _storage.DidNotReceiveWithAnyArgs().UploadPictureAsync(default, default, default, default);
        }

        [Test]
        public void UploadOriginal_ShouldReject_AnimatedWebp_BecauseThisImageSharpVersionCannotDecodeIt()
        {
            // GIF has a frame budget because ImageSharp decodes every frame. WebP needs none today: ImageSharp
            // 2.1 throws NotSupportedException ("Animated webp are not yet supported") on Identify and on Load,
            // so the upload is refused and the anonymous resize path cannot decode such a file either. If an
            // ImageSharp upgrade starts decoding animated WebP, this test fails and a frame budget is due.
            var stream = AnimatedWebp(frames: 50, width: 4, height: 4);

            Assert.That(() => _pictureService.UploadOriginalAsync(stream, "image/webp", "anim.webp", 2),
                Throws.ArgumentException.With.Message.Contains("not recognized"));
            _storage.DidNotReceiveWithAnyArgs().UploadPictureAsync(default, default, default, default);
            stream.Position = 0;
            Assert.That(() => Image.Load(stream), Throws.TypeOf<System.NotSupportedException>());
        }

        /// <summary>A valid animated WebP: VP8X with the animation flag, ANIM, and one ANMF per frame wrapping a lossless bitstream.</summary>
        private static MemoryStream AnimatedWebp(int frames, int width, int height)
        {
            byte[] vp8l;
            using (var image = new Image<Rgba32>(width, height))
            using (var single = new MemoryStream())
            {
                image.SaveAsWebp(single, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { FileFormat = SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless });
                vp8l = RiffChunk(single.ToArray(), "VP8L");
            }

            var body = new MemoryStream();
            void Fourcc(string s) => body.Write(Encoding.ASCII.GetBytes(s));
            void U32(uint v) => body.Write(System.BitConverter.GetBytes(v));
            void U24(int v) { body.WriteByte((byte)(v & 0xFF)); body.WriteByte((byte)((v >> 8) & 0xFF)); body.WriteByte((byte)((v >> 16) & 0xFF)); }

            Fourcc("WEBP");
            Fourcc("VP8X"); U32(10); body.WriteByte(0x02); body.WriteByte(0); body.WriteByte(0); body.WriteByte(0); U24(width - 1); U24(height - 1);
            Fourcc("ANIM"); U32(6); U32(0); body.WriteByte(0); body.WriteByte(0);
            for (var i = 0; i < frames; i++)
            {
                Fourcc("ANMF"); U32((uint)(16 + vp8l.Length));
                U24(0); U24(0); U24(width - 1); U24(height - 1); U24(50); body.WriteByte(0);
                body.Write(vp8l);
            }

            var result = new MemoryStream();
            result.Write(Encoding.ASCII.GetBytes("RIFF"));
            result.Write(System.BitConverter.GetBytes((uint)body.Length));
            body.Position = 0;
            body.CopyTo(result);
            result.Position = 0;
            return result;
        }

        private static byte[] RiffChunk(byte[] file, string fourcc)
        {
            var pos = 12;
            while (pos + 8 <= file.Length)
            {
                var name = Encoding.ASCII.GetString(file, pos, 4);
                var size = System.BitConverter.ToUInt32(file, pos + 4);
                var total = 8 + (int)size + (int)(size & 1);
                if (name == fourcc)
                {
                    return file[pos..(pos + total)];
                }

                pos += total;
            }

            throw new System.InvalidOperationException(fourcc + " chunk not found");
        }

        [TestCase("noextension")]
        [TestCase("victim.html")]
        public async Task RemoveImage_ShouldNotTouchStorage_WhenKeyIsNotAnImageName(string key)
        {
            await _pictureService.RemoveImageAsync(key, 2);

            await _storage.DidNotReceiveWithAnyArgs().RemovePictureAsync(default, default);
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
            _references.IsReferencedAsync("victim.jpg").Returns(Task.FromResult(true));

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

        [Test]
        public async Task IsInUse_ReflectsExistingReferences()
        {
            _references.IsReferencedAsync("taken.jpg").Returns(Task.FromResult(true));

            Assert.That(await _pictureService.IsInUseAsync("taken.jpg"), Is.True);
            Assert.That(await _pictureService.IsInUseAsync("fresh.jpg"), Is.False);
            Assert.That(await _pictureService.IsInUseAsync(null), Is.False);
        }

        [Test]
        public async Task RemoveImageIgnoringReferences_DeletesEvenWhenAnotherRecordUsesThePicture()
        {
            _references.IsReferencedAsync("victim.jpg").Returns(Task.FromResult(true));

            await _pictureService.RemoveImageIgnoringReferencesAsync("victim.jpg", 2);

            await _storage.Received(1).RemovePictureAsync("victim.jpg", "pictures");
        }

        [TestCase("../../appsettings.json")]
        [TestCase("victim.html")]
        [TestCase("")]
        public async Task RemoveImageIgnoringReferences_StillRefusesKeysThatAreNotStoredImages(string key)
        {
            await _pictureService.RemoveImageIgnoringReferencesAsync(key, 2);

            await _storage.DidNotReceiveWithAnyArgs().RemovePictureAsync(default, default);
        }
    }
}
