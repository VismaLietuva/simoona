using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;

namespace Shrooms.Domain.Services.Wall.Posts.ScheduledPosts
{
    public interface IScheduledPostService
    {
        Task<IEnumerable<ScheduledPostDto>> GetScheduledPostsAsync(int? wallId, UserAndOrganizationDto userOrg);

        Task<ScheduledPostDto> CreateAsync(NewScheduledPostDto dto);

        Task<ScheduledPostDto> EditAsync(EditScheduledPostDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);

        Task<NewlyCreatedPostDto> PublishNowAsync(int id, UserAndOrganizationDto userOrg);

        Task<NewlyCreatedPostDto> PublishDueAsync(int id);
    }
}
