using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Wall.Posts.ScheduledPosts
{
    public interface IScheduledPostPublisher
    {
        Task PublishAsync(int scheduledPostId, int organizationId, string tenantName);
    }
}
