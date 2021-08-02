using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Picture
{
    public interface IPictureService
    {
        Task<string> UploadFromImageAsync(Image image, string mimeType, string fileName, int orgId);

        Task<string> UploadFromStreamAsync(Stream stream, string mimeType, string fileName, int orgId);

        Task RemoveImageAsync(string blobKey, int orgId);
    }
}