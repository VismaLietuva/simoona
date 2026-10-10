using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Picture
{
    public interface IPictureService
    {
        // Legacy upload path used by the AngularJS UI. New consumers (Next.js with
        // next/image) should call UploadOriginalAsync instead.
        // Remove after new UI release.
        Task<string> UploadFromStreamAsync(Stream stream, string mimeType, string fileName, int orgId);

        // Stores the upload as-is after lightweight format validation. Intended for
        // clients that handle their own responsive sizing/encoding (e.g. Next.js
        // next/image), where any server-side re-encode is a quality loss.
        Task<string> UploadOriginalAsync(Stream stream, string mimeType, string fileName, int orgId);

        /// <summary>
        /// Deletes the picture unless another stored record still references it. Callers save their own
        /// change first and remove the previous picture afterwards.
        /// </summary>
        Task RemoveImageAsync(string blobKey, int orgId);

        /// <summary>
        /// Deletes the picture even if other records reference it. For the owner's personal data (user
        /// anonymization): a stranger who pointed their own record at this key must not be able to keep it alive.
        /// </summary>
        Task RemoveImageIgnoringReferencesAsync(string blobKey, int orgId);

        /// <summary>
        /// True when some stored record already points at this key. Picture keys are public, so a record
        /// taking a key as its own must take a fresh upload, never a key another record already uses.
        /// </summary>
        Task<bool> IsInUseAsync(string blobKey);
    }
}