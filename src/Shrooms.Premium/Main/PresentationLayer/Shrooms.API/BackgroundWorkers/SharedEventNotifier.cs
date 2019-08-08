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

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
{
    public class SharedEventNotifier : IBackgroundWorker
    {
        private readonly IWallService _wallService;
        public SharedEventNotifier(IWallService wallService)
        {
            _wallService = wallService;
        }

        public void Notify(NewPostDTO postModel, NewlyCreatedPostDTO createdPost, UserAndOrganizationHubDto userHubDto)
        {
            var membersToNotify = _wallService.GetWallMembersIds(postModel.WallId, postModel);
            NotificationHub.SendWallNotification(postModel.WallId, membersToNotify, createdPost.WallType, userHubDto);
        }
    }
}
