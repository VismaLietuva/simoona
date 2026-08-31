using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;

namespace Shrooms.Domain.Services.VideoLibrary
{
    public interface IVideoTypeService
    {
        Task<IEnumerable<VideoTypeDto>> GetVideoTypesAsync(UserAndOrganizationDto userOrg);

        Task CreateVideoTypeAsync(VideoTypeDto videoType);

        Task UpdateVideoTypeAsync(VideoTypeDto videoType);

        Task RemoveVideoTypeAsync(int id, UserAndOrganizationDto userOrg);
    }
}
