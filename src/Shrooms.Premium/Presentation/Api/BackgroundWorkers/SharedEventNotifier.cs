using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Wall;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Presentation.Api.BackgroundWorkers
{
  public class SharedEventNotifier : IBackgroundWorker
  {
    private readonly IWallService _wallService;
    private readonly IPostNotificationService _postNotificationService;

    public SharedEventNotifier(IWallService wallService, IPostNotificationService postNotificationService)
    {
      _wallService = wallService;
      _postNotificationService = postNotificationService;
    }

    public async Task NotifyAsync(NewPostDTO postModel, NewlyCreatedPostDTO createdPost, UserAndOrganizationHubDto userHubDto)
    {
      await _postNotificationService.NotifyAboutNewPostAsync(createdPost);

      var membersToNotify = await _wallService.GetWallMembersIdsAsync(postModel.WallId, postModel);
      NotificationHub.SendWallNotification(postModel.WallId, membersToNotify, createdPost.WallType, userHubDto);
    }
  }
}
