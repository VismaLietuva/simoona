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

        Task RemoveImageAsync(string blobKey, int orgId);
    }
}