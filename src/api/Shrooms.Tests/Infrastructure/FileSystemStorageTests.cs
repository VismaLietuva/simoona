using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Infrastructure.Storage.FileSystem;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class FileSystemStorageTests
    {
        private const string Tenant = "visma";

        private string _contentRoot;
        private string _tenantFolder;
        private string _secretOutsideStorage;
        private FileSystemStorage _storage;

        [SetUp]
        public void SetUp()
        {
            _contentRoot = Path.Combine(Path.GetTempPath(), "simoona-storage-tests", Guid.NewGuid().ToString("N"));
            _tenantFolder = Path.Combine(_contentRoot, "storage", Tenant);
            Directory.CreateDirectory(_tenantFolder);

            // Stand-in for appsettings.json: lives next to the storage folder, must never be reachable.
            _secretOutsideStorage = Path.Combine(_contentRoot, "appsettings.json");
            File.WriteAllText(_secretOutsideStorage, "{ \"JwtSecret\": \"secret\" }");

            var environment = Substitute.For<IWebHostEnvironment>();
            environment.ContentRootPath.Returns(_contentRoot);

            _storage = new FileSystemStorage(environment);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_contentRoot))
            {
                Directory.Delete(_contentRoot, recursive: true);
            }
        }

        [Test]
        public async Task UploadGetRemove_RoundTripsInsideTenantFolder()
        {
            const string key = "746afdc0-ea3b-4a94-836e-5e3e7774bca3.jpg";
            var payload = Encoding.UTF8.GetBytes("image-bytes");

            await _storage.UploadPictureAsync(new MemoryStream(payload), key, "image/jpeg", Tenant);

            Assert.That(File.Exists(Path.Combine(_tenantFolder, key)), Is.True);

            await using (var stream = await _storage.GetPictureAsync(key, Tenant))
            {
                Assert.That(stream, Is.Not.Null);
                using var reader = new StreamReader(stream);
                Assert.That(await reader.ReadToEndAsync(), Is.EqualTo("image-bytes"));
            }

            await _storage.RemovePictureAsync(key, Tenant);

            Assert.That(File.Exists(Path.Combine(_tenantFolder, key)), Is.False);
        }

        [TestCase("../../appsettings.json")]
        [TestCase("..\\..\\appsettings.json")]
        [TestCase("../../../appsettings.json")]
        public void RemovePictureAsync_TraversalKey_ThrowsAndLeavesFileIntact(string key)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _storage.RemovePictureAsync(key, Tenant));

            Assert.That(File.Exists(_secretOutsideStorage), Is.True);
        }

        [TestCase("../../appsettings.json")]
        [TestCase("..\\..\\appsettings.json")]
        public void GetPictureAsync_TraversalKey_Throws(string key)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _storage.GetPictureAsync(key, Tenant));
        }

        [Test]
        public void GetPictureAsync_TraversalTenant_Throws()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _storage.GetPictureAsync("appsettings.json", ".."));
        }

        [Test]
        public void UploadPictureAsync_TraversalKey_DoesNotWriteOutsideStorage()
        {
            var target = Path.Combine(_contentRoot, "planted.txt");

            Assert.ThrowsAsync<ArgumentException>(() =>
                _storage.UploadPictureAsync(new MemoryStream(new byte[] { 1 }), "../planted.txt", "text/plain", Tenant));

            Assert.That(File.Exists(target), Is.False);
        }

        [Test]
        public async Task GetPictureAsync_MissingFile_ReturnsNull()
        {
            var stream = await _storage.GetPictureAsync("00000000-0000-0000-0000-000000000000.jpg", Tenant);

            Assert.That(stream, Is.Null);
        }
    }
}
