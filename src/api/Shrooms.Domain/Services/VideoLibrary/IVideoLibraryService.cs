using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;

namespace Shrooms.Domain.Services.VideoLibrary
{
    public interface IVideoLibraryService
    {
        Task<IEnumerable<VideoLibraryItemDto>> GetVideosAsync(UserAndOrganizationDto userOrg);

        Task CreateVideoAsync(VideoLibraryItemDto video);

        Task UpdateVideoAsync(VideoLibraryItemDto video);

        Task RemoveVideoAsync(int id, UserAndOrganizationDto userOrg);
    }
}
