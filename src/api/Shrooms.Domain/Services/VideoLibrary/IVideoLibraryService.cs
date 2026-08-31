using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using X.PagedList;

namespace Shrooms.Domain.Services.VideoLibrary
{
    public interface IVideoLibraryService
    {
        Task<IPagedList<VideoLibraryItemDto>> GetVideosAsync(VideoLibraryListArgsDto args);

        Task<VideoLibraryFiltersDto> GetFiltersAsync(UserAndOrganizationDto userOrg);

        Task CreateVideoAsync(VideoLibraryItemDto video);

        Task UpdateVideoAsync(VideoLibraryItemDto video);

        Task RemoveVideoAsync(int id, UserAndOrganizationDto userOrg);
    }
}
