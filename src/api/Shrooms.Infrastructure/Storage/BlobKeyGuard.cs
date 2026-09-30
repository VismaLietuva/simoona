using System;
using System.IO;
using System.Linq;

namespace Shrooms.Infrastructure.Storage
{
    /// <summary>
    /// Validates the blob key (file name) and container (tenant) segments before they reach a storage provider.
    /// Blob keys come from the client (picture ids on profiles, posts, comments) and from anonymous URLs, so
    /// anything that is not a bare file name made of safe characters is rejected: no path separators, no
    /// "." or ".." segments, no rooted paths. Legitimate keys are GUID + image extension.
    /// </summary>
    public static class BlobKeyGuard
    {
        public const int MaxBlobKeyLength = 255;
        public const int MaxContainerLength = 63;

        public static bool IsSafeBlobKey(string blobKey)
        {
            if (string.IsNullOrWhiteSpace(blobKey) || blobKey.Length > MaxBlobKeyLength)
            {
                return false;
            }

            if (blobKey[0] == '.' || blobKey.Contains("..", StringComparison.Ordinal))
            {
                return false;
            }

            if (!blobKey.All(IsSafeBlobKeyChar))
            {
                return false;
            }

            return Path.GetFileName(blobKey) == blobKey && !Path.IsPathRooted(blobKey);
        }

        public static bool IsSafeContainer(string container)
        {
            if (string.IsNullOrWhiteSpace(container) || container.Length > MaxContainerLength)
            {
                return false;
            }

            return container.All(IsSafeContainerChar);
        }

        public static void EnsureSafeBlobKey(string blobKey)
        {
            if (!IsSafeBlobKey(blobKey))
            {
                throw new ArgumentException("Blob key must be a bare file name without path separators.", nameof(blobKey));
            }
        }

        public static void EnsureSafeContainer(string container)
        {
            if (!IsSafeContainer(container))
            {
                throw new ArgumentException("Container name contains unsupported characters.", nameof(container));
            }
        }

        private static bool IsSafeBlobKeyChar(char c)
        {
            return (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || (c >= '0' && c <= '9')
                || c == '-' || c == '_' || c == '.';
        }

        private static bool IsSafeContainerChar(char c)
        {
            return (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || (c >= '0' && c <= '9')
                || c == '-' || c == '_';
        }
    }
}
