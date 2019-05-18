using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Email.Posting;
using Shrooms.Domain.Services.Permissions;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.EntityModels.Models.Notifications;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Infrastructure.SystemClock;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;
        private readonly ICommentNotificationService _commentNotificationService;

        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<Comment> _commentsDbSet;
        private readonly IDbSet<WallModerator> _wallModeratorsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<NotificationsSettings> _notificationsDbSet;

        public CommentService(
            IUnitOfWork2 uow,
            ISystemClock systemClock,
            IPermissionService permissionService,
            ICommentNotificationService commentNotificationService)
        {
            _uow = uow;
            _systemClock = systemClock;
            _permissionService = permissionService;

            _postsDbSet = uow.GetDbSet<Post>();
            _commentsDbSet = uow.GetDbSet<Comment>();
            _wallModeratorsDbSet = uow.GetDbSet<WallModerator>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _notificationsDbSet = uow.GetDbSet<NotificationsSettings>();
            _commentNotificationService = commentNotificationService;
        }

        public void ToggleLike(int commentId, UserAndOrganizationDTO userOrg)
        {
            var comment = _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefault(x =>
                    x.Id == commentId &&
                    x.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var like = comment.Likes.FirstOrDefault(x => x.UserId == userOrg.UserId);
            if (like == null)
            {
                comment.Likes.Add(new Like(userOrg.UserId));
            }
            else
            {
                comment.Likes.Remove(like);
            }

            _uow.SaveChanges(userOrg.UserId);
        }

        public IList<string> GetCommentsAuthorsToNotify(int postId, IEnumerable<string> excludeUsers)
        {
            var comments = _commentsDbSet
                .Where(comment => postId == comment.PostId && comment.AuthorId != null)
                .OrderBy(o => o.Created)
                .ToList();

            var commentsAuthorsToNotify = comments
                    .Where(c => excludeUsers.Contains(c.AuthorId) == false && c.Author != null)
                    .Where(c => FollowingPostsAppNotificationsEnabled(c.Author.Id))
                    .Select(c => c.AuthorId).Distinct().ToList();

            return commentsAuthorsToNotify;
        }

        public CommentCreatedDTO CreateComment(NewCommentDTO commentDto)
        {
            var post = _postsDbSet
                .Include(x => x.Wall)
                .FirstOrDefault(p =>
                    p.Id == commentDto.PostId && p.Wall.OrganizationId == commentDto.OrganizationId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post does not exist");
            }

            var comment = new Comment
            {
                AuthorId = commentDto.UserId,
                MessageBody = commentDto.MessageBody,
                PictureId = commentDto.PictureId,
                PostId = commentDto.PostId,
                Likes = new LikesCollection(),
                LastEdit = DateTime.UtcNow
            };
            _commentsDbSet.Add(comment);

            post.LastActivity = _systemClock.UtcNow;
            _uow.SaveChanges(commentDto.UserId);

            _commentNotificationService.NotifyAboutNewComment(comment, _usersDbSet.SingleOrDefault(user => user.Id == commentDto.UserId));

            return new CommentCreatedDTO { WallId = post.WallId, CommentId = comment.Id, WallType = post.Wall.Type, CommentCreator = comment.AuthorId, PostCreator = post.AuthorId, PostId = post.Id };
        }

        public void EditComment(EditCommentDTO commentDto)
        {
            var comment = _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefault(c =>
                    c.Id == commentDto.Id &&
                    c.Post.Wall.OrganizationId == commentDto.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = _wallModeratorsDbSet
                .Any(x => x.UserId == commentDto.UserId && x.WallId == comment.Post.WallId) || commentDto.UserId == comment.CreatedBy;

            var isAdministrator = _permissionService.UserHasPermission(commentDto, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            comment.MessageBody = commentDto.MessageBody;
            comment.PictureId = commentDto.PictureId;
            comment.LastEdit = DateTime.UtcNow;
            _uow.SaveChanges(commentDto.UserId);
        }

        public void DeleteComment(int commentId, UserAndOrganizationDTO userOrg)
        {
            var comment = _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefault(c =>
                    c.Id == commentId &&
                    c.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = _wallModeratorsDbSet
                .Any(x => x.UserId == userOrg.UserId && x.WallId == comment.Post.WallId) || comment.CreatedBy == userOrg.UserId;

            var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            _commentsDbSet.Remove(comment);
            _uow.SaveChanges(userOrg.UserId);
        }

        public void HideComment(int commentId, UserAndOrganizationDTO userOrg)
        {
            var comment = _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefault(c =>
                    c.Id == commentId &&
                    c.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = _wallModeratorsDbSet
                .Any(x => x.UserId == userOrg.UserId && x.WallId == comment.Post.WallId) || comment.CreatedBy == userOrg.UserId;

            var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            comment.IsHidden = true;
            comment.LastEdit = DateTime.UtcNow;

            _uow.SaveChanges(userOrg.UserId);
        }

        public void DeleteCommentsByPost(int postId, UserAndOrganizationDTO userAndOrg)
        {
            var comments = _commentsDbSet
                .Where(x => x.PostId == postId)
                .ToList();

            comments.ForEach(x => _commentsDbSet.Remove(x));
        }

        public bool IsPostAuthorAppNotificationsEnabled(string userId)
        {
            return _notificationsDbSet
                .Where(x => x.ApplicationUser.Id == userId)
                .Select(x => x.MyPostsAppNotifications)
                .DefaultIfEmpty(true)
                .SingleOrDefault();
        }

        private bool FollowingPostsAppNotificationsEnabled(string userId)
        {
            return _notificationsDbSet
                .Where(x => x.ApplicationUser.Id == userId)
                .Select(x => x.FollowingPostsAppNotifications)
                .DefaultIfEmpty(true)
                .SingleOrDefault();
        }
    }
}