using System;
using NUnit.Framework;
using Shrooms.Infrastructure.Storage;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class BlobKeyGuardTests
    {
        [TestCase("746afdc0-ea3b-4a94-836e-5e3e7774bca3.jpg")]
        [TestCase("746AFDC0-EA3B-4A94-836E-5E3E7774BCA3.PNG")]
        [TestCase("legacy_name-1.jpeg")]
        [TestCase("noextension")]
        public void IsSafeBlobKey_AcceptsBareFileNames(string key)
        {
            Assert.That(BlobKeyGuard.IsSafeBlobKey(key), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("..")]
        [TestCase(".")]
        [TestCase(".hidden")]
        [TestCase("../../appsettings.json")]
        [TestCase("..\\..\\appsettings.json")]
        [TestCase("folder/file.jpg")]
        [TestCase("folder\\file.jpg")]
        [TestCase("/etc/passwd")]
        [TestCase("C:\\Windows\\win.ini")]
        [TestCase("file..jpg")]
        [TestCase("file name.jpg")]
        [TestCase("file\0.jpg")]
        [TestCase("file%2F.jpg")]
        public void IsSafeBlobKey_RejectsTraversalAndUnsafeCharacters(string key)
        {
            Assert.That(BlobKeyGuard.IsSafeBlobKey(key), Is.False);
        }

        [Test]
        public void IsSafeBlobKey_RejectsOverlongKeys()
        {
            var key = new string('a', BlobKeyGuard.MaxBlobKeyLength + 1);

            Assert.That(BlobKeyGuard.IsSafeBlobKey(key), Is.False);
        }

        [TestCase("visma")]
        [TestCase("simoona-test")]
        [TestCase("org_1")]
        public void IsSafeContainer_AcceptsTenantNames(string container)
        {
            Assert.That(BlobKeyGuard.IsSafeContainer(container), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("..")]
        [TestCase("../visma")]
        [TestCase("visma/other")]
        [TestCase("visma\\other")]
        [TestCase("visma.other")]
        public void IsSafeContainer_RejectsSeparatorsAndDots(string container)
        {
            Assert.That(BlobKeyGuard.IsSafeContainer(container), Is.False);
        }

        [Test]
        public void EnsureSafeBlobKey_ThrowsForTraversal()
        {
            Assert.Throws<ArgumentException>(() => BlobKeyGuard.EnsureSafeBlobKey("../secret.txt"));
        }

        [Test]
        public void EnsureSafeContainer_ThrowsForTraversal()
        {
            Assert.Throws<ArgumentException>(() => BlobKeyGuard.EnsureSafeContainer("../tenant"));
        }
    }
}
