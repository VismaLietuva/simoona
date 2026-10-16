using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Wall.Posts.ScheduledPosts;
using Shrooms.Infrastructure.FireAndForget;

namespace Shrooms.Presentation.Api.BackgroundWorkers
{
    public class ScheduledPostPublisher : IScheduledPostPublisher
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ScheduledPostPublisher(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task PublishAsync(int scheduledPostId, int organizationId, string tenantName)
        {
            using var scope = _scopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantNameContainer>().TenantName = tenantName;

            var createdPost = await scope.ServiceProvider.GetRequiredService<IScheduledPostService>().PublishDueAsync(scheduledPostId);

            if (createdPost == null)
            {
                return;
            }

            var userHubDto = new UserAndOrganizationHubDto
            {
                UserId = createdPost.User.UserId,
                OrganizationId = organizationId,
                OrganizationName = tenantName
            };

            await scope.ServiceProvider.GetRequiredService<PostNotifier>().NotifyAboutNewPostAsync(createdPost, userHubDto);
        }
    }
}
