using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Infrastructure.Storage
{
    public interface IStorage
    {
        Task UploadPictureAsync(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer);

        Task RemovePictureAsync(string blobKey, string tenantPicturesContainer);

        Task<Stream> GetPictureAsync(string blobKey, string tenantPicturesContainer);
    }
}