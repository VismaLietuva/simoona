using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Infrastructure.Logger;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Wall;
using Shrooms.API.Hubs;
using Shrooms.Domain.Services.Email.Posting;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
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
