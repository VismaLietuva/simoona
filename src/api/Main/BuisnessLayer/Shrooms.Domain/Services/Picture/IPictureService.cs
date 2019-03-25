using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Picture
{
    public interface IPictureService
    {
        Task<string> UploadFromStream(Stream stream, string mimeType, string fileName, int orgId);
    }
}