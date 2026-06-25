using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;
using Shrooms.Infrastructure.Storage;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PictureServiceTests
    {
        // Minimal magic-byte headers — enough to satisfy the format sniff in UploadOriginalAsync
        // without pulling an image library into the test project.
        private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] GifHeader = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        private static readonly byte[] WebpHeader = { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };

        private IPictureService _pictureService;
        private IStorage _storage;
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
            _pictureService = new PictureService(_storage, uow);
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenJpg()
        {
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.jpg", 2);

            Assert.That(result, Does.EndWith(".jpg"));
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenPng()
        {
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.png", 2);

            Assert.That(result, Does.EndWith(".png"));
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenGif()
        {
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.gif", 2);

            Assert.That(result, Does.EndWith(".gif"));
        }

        [Test]
        public async Task UploadOriginal_ShouldAcceptJpeg()
        {
            using var stream = new MemoryStream(JpegHeader);

            var result = await _pictureService.UploadOriginalAsync(stream, "image/jpeg", "photo.jpg", 2);

            Assert.That(result, Does.EndWith(".jpg"));
        }

        [Test]
        public async Task UploadOriginal_ShouldAcceptPng()
        {
            using var stream = new MemoryStream(PngHeader);

            var result = await _pictureService.UploadOriginalAsync(stream, "image/png", "logo.png", 2);

            Assert.That(result, Does.EndWith(".png"));
        }

        [Test]
        public async Task UploadOriginal_ShouldAcceptGif()
        {
            using var stream = new MemoryStream(GifHeader);

            var result = await _pictureService.UploadOriginalAsync(stream, "image/gif", "still.gif", 2);

            Assert.That(result, Does.EndWith(".gif"));
        }

        [Test]
        public async Task UploadOriginal_ShouldAcceptWebp()
        {
            using var stream = new MemoryStream(WebpHeader);

            var result = await _pictureService.UploadOriginalAsync(stream, "image/webp", "modern.webp", 2);

            Assert.That(result, Does.EndWith(".webp"));
        }

        [Test]
        public async Task UploadOriginal_ShouldPassOriginalMimeType_ToStorage()
        {
            using var stream = new MemoryStream(JpegHeader);

            await _pictureService.UploadOriginalAsync(stream, "image/jpeg", "photo.jpg", 2);

            await _storage.Received(1).UploadPictureAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                "image/jpeg",
                "pictures");
        }

        [Test]
        public void UploadOriginal_ShouldThrow_WhenStreamIsNotARecognizedImage()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            Assert.That(
                async () => await _pictureService.UploadOriginalAsync(stream, "image/jpeg", "garbage.jpg", 2),
                Throws.ArgumentException);
        }
    }
}
