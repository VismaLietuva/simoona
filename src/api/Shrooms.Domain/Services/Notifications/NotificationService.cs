using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Notification;
using Shrooms.Contracts.DataTransferObjects.Models.Wall;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Domain.Services.Wall;

namespace Shrooms.Domain.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IDbSet<Notification> _notificationDbSet;
        private readonly IDbSet<NotificationUser> _notificationUserDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IDbSet<DataLayer.EntityModels.Models.Multiwall.Wall> _wallDbSet;
        private readonly IDbSet<Event> _eventDbSet;
        private readonly IDbSet<Project> _projectDbSet;

        private readonly IWallService _wallService;

        private readonly IMapper _mapper;

        private readonly IUnitOfWork2 _uow;

        public NotificationService(IUnitOfWork2 uow, IMapper mapper, IWallService wallService)
        {
            _notificationDbSet = uow.GetDbSet<Notification>();
            _notificationUserDbSet = uow.GetDbSet<NotificationUser>();
            _wallDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _eventDbSet = uow.GetDbSet<Event>();
            _projectDbSet = uow.GetDbSet<Project>();
            _uow = uow;

            _mapper = mapper;

            _wallService = wallService;
        }

        public async Task<NotificationDto> CreateForPost(UserAndOrganizationDTO userOrg, NewlyCreatedPostDTO post, int wallId, IEnumerable<string> membersToNotify)
        {
            var postType = NotificationType.WallPost;
            var sources = new Sources { PostId = post.Id };

            switch (post.WallType)
            {
                case WallType.Events:
                    postType = NotificationType.EventPost;
                    sources.EventId = _eventDbSet.FirstOrDefault(x => x.WallId == wallId).Id.ToString();
                    break;
                case WallType.Project:
                    postType = NotificationType.ProjectPost;
                    sources.ProjectId = _projectDbSet.FirstOrDefault(x => x.WallId == wallId).Id.ToString();
                    break;
            }

            var wallName = await _wallDbSet
                               .Where(x => x.Id == wallId && x.OrganizationId == userOrg.OrganizationId)
                               .Select(s => s.Name)
                               .SingleAsync();

            var newNotification = Notification.Create(post.User.FullName, wallName, post.User.PictureId, sources, postType, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }

        public async Task<NotificationDto> CreateForComment(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, string memberToNotify)
        {
            return await CreateForComment(userOrg, comment, type, new List<string> { memberToNotify });
        }

        public async Task<NotificationDto> CreateForComment(UserAndOrganizationDTO userOrg, CommentCreatedDTO comment, NotificationType type, IEnumerable<string> membersToNotify)
        {
            var sources = new Sources { PostId = comment.PostId };

            var wallName = await _wallDbSet
                               .Where(x => x.Id == comment.WallId && x.OrganizationId == userOrg.OrganizationId)
                               .Select(s => s.Name)
                               .SingleAsync();

            var commentCreator = await _userDbSet
                                     .Where(x => x.Id == comment.CommentCreator)
                                     .Select(c => new { c.FirstName, c.LastName, c.PictureId })
                                     .SingleAsync();
            var commentCreatorApplicationUser = new ApplicationUser
            {
                FirstName = commentCreator.FirstName,
                LastName = commentCreator.LastName,
                PictureId = commentCreator.PictureId
            };

            var newNotification = Notification.Create(commentCreatorApplicationUser.FullName, wallName, commentCreatorApplicationUser.PictureId, sources, type, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }

        public async Task<NotificationDto> CreateForWall(UserAndOrganizationDTO userOrg, CreateWallDto wallDto, int wallId)
        {
            int mainWallId = await _wallDbSet.Where(w => w.Type == WallType.Main).Select(s => s.Id).SingleAsync();

            var membersToNotify = _wallService.GetWallMembersIds(mainWallId, userOrg);

            var newNotification = Notification.Create(wallDto.Name, wallDto.Description, wallDto.Logo, new Sources { WallId = wallId }, NotificationType.NewWall, userOrg.OrganizationId, membersToNotify);

            _notificationDbSet.Add(newNotification);

            await _uow.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(newNotification);
        }

        public async Task<IEnumerable<NotificationDto>> GetAll(UserAndOrganizationDTO userOrg)
        {
            var result = await _notificationUserDbSet
                             .Include(x => x.Notification)
                             .Where(w => !w.IsAlreadySeen && w.UserId == userOrg.UserId)
                             .Select(s => s.Notification)
                             .OrderByDescending(o => o.Created)
                            .Take(() => BusinessLayerConstants.MaxNotificationsToShow)
                             .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationDto>>(result);
        }

        public async Task MarkAsRead(UserAndOrganizationDTO userOrg, IEnumerable<int> notificationIds)
        {
            var notificationUsers = _notificationUserDbSet
                .Include(i => i.Notification)
                .Where(s => notificationIds.Contains(s.NotificationId) &&
                            s.Notification.OrganizationId == userOrg.OrganizationId &&
                            s.UserId == userOrg.UserId &&
                            !s.IsAlreadySeen).ToList();

            if (notificationUsers.Count == 0)
            {
                return;
            }

            foreach (var user in notificationUsers)
            {
                user.IsAlreadySeen = true;
            }

            await _uow.SaveChangesAsync();
        }

        public async Task MarkAllAsRead(UserAndOrganizationDTO userOrg)
        {
            var notificationUsers = await _notificationUserDbSet
                                        .Include(i => i.Notification)
                                        .Where(s => s.Notification.OrganizationId == userOrg.OrganizationId &&
                                                    s.UserId == userOrg.UserId &&
                                                    !s.IsAlreadySeen)
                                        .ToListAsync();

            if (!notificationUsers.Any())
            {
                return;
            }

            foreach (var notificationUser in notificationUsers)
            {
                notificationUser.IsAlreadySeen = true;
            }

            await _uow.SaveChangesAsync();
        }
    }
}