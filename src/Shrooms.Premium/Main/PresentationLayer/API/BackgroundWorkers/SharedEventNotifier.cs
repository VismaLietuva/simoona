using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Wall;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Main.PresentationLayer.API.BackgroundWorkers
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

    public void Notify(NewPostDTO postModel, NewlyCreatedPostDTO createdPost, UserAndOrganizationHubDto userHubDto)
    {
      _postNotificationService.NotifyAboutNewPost(createdPost);

      var membersToNotify = _wallService.GetWallMembersIds(postModel.WallId, postModel);
      NotificationHub.SendWallNotification(postModel.WallId, membersToNotify, createdPost.WallType, userHubDto);
    }
  }
}
