using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly DbSet<Post> _postsDbSet;
        private readonly DbSet<Comment> _commentsDbSet;
        private readonly DbSet<WallModerator> _wallModeratorsDbSet;
        private readonly DbSet<PostWatcher> _postWatchers;

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

        public async Task ToggleLikeAsync(int commentId, UserAndOrganizationDto userOrg)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefaultAsync(x => x.Id == commentId && x.Post.Wall.OrganizationId == userOrg.OrganizationId);

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

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<CommentCreatedDto> CreateCommentAsync(NewCommentDto commentDto)
        {
            var post = await _postsDbSet
                .Include(x => x.Wall)
                .FirstOrDefaultAsync(p => p.Id == commentDto.PostId && p.Wall.OrganizationId == commentDto.OrganizationId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post does not exist");
            }

            var watchEntity = await _postWatchers.FindAsync(commentDto.PostId, commentDto.UserId);

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

            await _uow.SaveChangesAsync(commentDto.UserId);

            return new CommentCreatedDto
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

        public async Task EditCommentAsync(EditCommentDto commentDto)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefaultAsync(c => c.Id == commentDto.Id && c.Post.Wall.OrganizationId == commentDto.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = await _wallModeratorsDbSet
                .AnyAsync(x => x.UserId == commentDto.UserId && x.WallId == comment.Post.WallId) || commentDto.UserId == comment.CreatedBy;

            var isAdministrator = await _permissionService.UserHasPermissionAsync(commentDto, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            comment.MessageBody = commentDto.MessageBody;
            comment.PictureId = commentDto.PictureId;
            comment.LastEdit = DateTime.UtcNow;

            await _uow.SaveChangesAsync(commentDto.UserId);
        }

        public async Task DeleteCommentAsync(int commentId, UserAndOrganizationDto userOrg)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefaultAsync(c =>
                    c.Id == commentId &&
                    c.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = await _wallModeratorsDbSet
                .AnyAsync(x => x.UserId == userOrg.UserId && x.WallId == comment.Post.WallId) || comment.CreatedBy == userOrg.UserId;

            var isAdministrator = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            _commentsDbSet.Remove(comment);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<string> GetCommentBodyAsync(int commentId)
        {
            return (await _commentsDbSet.FirstOrDefaultAsync(x => x.Id == commentId))?.MessageBody;
        }

        public async Task HideCommentAsync(int commentId, UserAndOrganizationDto userOrg)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            var isWallModerator = await _wallModeratorsDbSet
                .AnyAsync(x => x.UserId == userOrg.UserId && x.WallId == comment.Post.WallId) || comment.CreatedBy == userOrg.UserId;

            var isAdministrator = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post);

            if (!isAdministrator && !isWallModerator)
            {
                throw new UnauthorizedException();
            }

            comment.IsHidden = true;
            comment.LastEdit = DateTime.UtcNow;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task DeleteCommentsByPostAsync(int postId)
        {
            var comments = await _commentsDbSet
                .Where(x => x.PostId == postId)
                .ToListAsync();

            comments.ForEach(x => _commentsDbSet.Remove(x));
        }
    }
}
