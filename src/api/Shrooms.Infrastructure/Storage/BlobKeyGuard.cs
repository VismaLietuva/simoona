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

        /// <summary>
        /// The only extensions a picture may be stored under, and the only ones the anonymous storage
        /// endpoint will serve. Keeps client-controlled names like "x.html" or "x.svg" from ever becoming
        /// a document that renders on the API origin.
        /// </summary>
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".bmp", ".webp" };

        public static bool HasAllowedImageExtension(string blobKey)
        {
            if (string.IsNullOrEmpty(blobKey))
            {
                return false;
            }

            var extension = Path.GetExtension(blobKey);
            return extension.Length > 0
                && Array.Exists(AllowedImageExtensions, e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Maps an allowlisted image media type to the extension used for stored keys, or null.</summary>
        public static string ExtensionForMimeType(string mimeType)
        {
            return mimeType?.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" or "image/pjpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" or "image/x-ms-bmp" => ".bmp",
                "image/webp" => ".webp",
                _ => null
            };
        }

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
