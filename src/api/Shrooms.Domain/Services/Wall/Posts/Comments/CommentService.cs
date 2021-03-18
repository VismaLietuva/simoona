using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;

        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<Comment> _commentsDbSet;
        private readonly IDbSet<WallModerator> _wallModeratorsDbSet;
        private readonly IDbSet<PostWatcher> _postWatchers;

        public CommentService(IUnitOfWork2 uow, ISystemClock systemClock, IPermissionService permissionService)
        {
            _uow = uow;
            _systemClock = systemClock;
            _permissionService = permissionService;

            _postsDbSet = uow.GetDbSet<Post>();
            _commentsDbSet = uow.GetDbSet<Comment>();
            _wallModeratorsDbSet = uow.GetDbSet<WallModerator>();
            _postWatchers = uow.GetDbSet<PostWatcher>();
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

        public CommentCreatedDTO CreateComment(NewCommentDTO commentDto)
        {
            var post = _postsDbSet
                .Include(x => x.Wall)
                .FirstOrDefault(p => p.Id == commentDto.PostId && p.Wall.OrganizationId == commentDto.OrganizationId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post does not exist");
            }

            var watchEntity = _postWatchers.Find(commentDto.PostId, commentDto.UserId);

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

            if (watchEntity == null)
            {
                _postWatchers.Add(new PostWatcher
                {
                    PostId = post.Id,
                    UserId = commentDto.UserId
                });
            }

            _uow.SaveChanges(commentDto.UserId);

            return new CommentCreatedDTO
            {
                WallId = post.WallId,
                CommentId = comment.Id,
                WallType = post.Wall.Type,
                CommentCreator = comment.AuthorId,
                PostCreator = post.AuthorId,
                PostId = post.Id,
                MentionedUsersIds = commentDto.MentionedUserIds?.Distinct()
            };
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

        public string GetCommentBody(int commentId)
        {
            return _commentsDbSet.FirstOrDefault(x => x.Id == commentId)?.MessageBody;
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

        public void DeleteCommentsByPost(int postId)
        {
            var comments = _commentsDbSet
                .Where(x => x.PostId == postId)
                .ToList();

            comments.ForEach(x => _commentsDbSet.Remove(x));
        }
    }
}