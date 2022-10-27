using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Contracts.Enums;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;
        private readonly IWallService _wallService;

        private readonly DbSet<Post> _postsDbSet;
        private readonly DbSet<Comment> _commentsDbSet;
        private readonly DbSet<WallModerator> _wallModeratorsDbSet;
        private readonly DbSet<PostWatcher> _postWatchers;

        public CommentService(IUnitOfWork2 uow, ISystemClock systemClock, IPermissionService permissionService, IWallService wallService)
        {
            _uow = uow;
            _systemClock = systemClock;
            _permissionService = permissionService;
            _wallService = wallService;

            _postsDbSet = uow.GetDbSet<Post>();
            _commentsDbSet = uow.GetDbSet<Comment>();
            _wallModeratorsDbSet = uow.GetDbSet<WallModerator>();
            _postWatchers = uow.GetDbSet<PostWatcher>();
        }

        public async Task ToggleLikeAsync(AddLikeDto addLikeDto, UserAndOrganizationDto userOrg)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post.Wall)
                .FirstOrDefaultAsync(x => x.Id == addLikeDto.Id && x.Post.Wall.OrganizationId == userOrg.OrganizationId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Comment does not exist");
            }

            await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                comment.Post.Wall,
                comment.AuthorId,
                BasicPermissions.Comment,
                userOrg,
                checkForAdministrationEventPermission: false);

            var like = comment.Likes.FirstOrDefault(x => x.UserId == userOrg.UserId);

            if (like == null)
            {
                comment.Likes.Add(new Like(userOrg.UserId, addLikeDto.Type));
            }
            else
            {
                comment.Likes.Remove(like);
            }

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<MentionCommentDto> GetMentionCommentByIdAsync(int commentId)
        {
            var comment = await _commentsDbSet
                .Include(comment => comment.Author)
                .SingleOrDefaultAsync(comment => comment.Id == commentId);
            
            if (comment == null)
            {
                return null;
            }

            return new MentionCommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                AuthorFullName = comment.Author.FullName
            };
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
                Images = new ImageCollection(commentDto.Images),
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
                CommentAuthor = comment.AuthorId,
                PostAuthor = post.AuthorId,
                PostId = post.Id,
                MentionedUserIds = commentDto.MentionedUserIds?.Distinct()
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

            await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                comment.Post.Wall,
                comment.CreatedBy,
                AdministrationPermissions.Post,
                userOrg: commentDto,
                checkForAdministrationEventPermission: true);

            comment.MessageBody = commentDto.MessageBody;
            comment.Images = new ImageCollection(commentDto.Images);
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

            await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                comment.Post.Wall,
                comment.CreatedBy,
                AdministrationPermissions.Post,
                userOrg,
                checkForAdministrationEventPermission: true);

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

            await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                comment.Post.Wall,
                comment.CreatedBy,
                AdministrationPermissions.Post,
                userOrg,
                checkForAdministrationEventPermission: true);

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
