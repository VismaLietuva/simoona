using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall.Posts.Comments;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public class PostService : IPostService
    {
        private static readonly SemaphoreSlim _postDeleteLock = new SemaphoreSlim(1, 1);

        private readonly IPermissionService _permissionService;
        private readonly ICommentService _commentService;

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallModerator> _moderatorsDbSet;
        private readonly IDbSet<DataLayer.EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly DbSet<PostWatcher> _postWatchers;

        public PostService(IUnitOfWork2 uow, IPermissionService permissionService, ICommentService commentService)
        {
            _uow = uow;
            _permissionService = permissionService;
            _commentService = commentService;

            _postsDbSet = uow.GetDbSet<Post>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _moderatorsDbSet = uow.GetDbSet<WallModerator>();
            _wallsDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _postWatchers = uow.GetDbSet<PostWatcher>();
        }

        public async Task<NewlyCreatedPostDto> CreateNewPostAsync(NewPostDto newPostDto)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var wall = await _wallsDbSet.FirstOrDefaultAsync(x => x.Id == newPostDto.WallId && x.OrganizationId == newPostDto.OrganizationId);

                if (wall == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
                }

                var post = new Post
                {
                    AuthorId = newPostDto.UserId,
                    Created = DateTime.UtcNow,
                    LastEdit = DateTime.UtcNow,
                    CreatedBy = newPostDto.UserId,
                    MessageBody = newPostDto.MessageBody,
                    PictureId = newPostDto.PictureId,
                    SharedEventId = newPostDto.SharedEventId,
                    LastActivity = DateTime.UtcNow,
                    WallId = newPostDto.WallId,
                    Likes = new LikesCollection()
                };

                _postsDbSet.Add(post);
                await _uow.SaveChangesAsync(newPostDto.UserId);

                _postWatchers.Add(new PostWatcher
                {
                    PostId = post.Id,
                    UserId = newPostDto.UserId
                });

                await _uow.SaveChangesAsync(newPostDto.UserId);

                var postCreator = await _usersDbSet.SingleAsync(user => user.Id == newPostDto.UserId);
                var postCreatorDto = MapUserToDto(postCreator);
                var newlyCreatedPostDto = MapNewlyCreatedPostToDto(post, postCreatorDto, wall.Type, newPostDto.MentionedUserIds);

                return newlyCreatedPostDto;
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        public async Task ToggleLikeAsync(int postId, UserAndOrganizationDto userOrg)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var post = await _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefaultAsync(x => x.Id == postId && x.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post does not exist");
                }

                var like = post.Likes.FirstOrDefault(x => x.UserId == userOrg.UserId);
                if (like == null)
                {
                    post.Likes.Add(new Like(userOrg.UserId));
                }
                else
                {
                    post.Likes.Remove(like);
                }

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        public async Task EditPostAsync(EditPostDto editPostDto)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var post = await _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefaultAsync(x =>
                        x.Id == editPostDto.Id &&
                        x.Wall.OrganizationId == editPostDto.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = await _moderatorsDbSet.AnyAsync(x => x.UserId == editPostDto.UserId && x.WallId == post.WallId) || post.CreatedBy == editPostDto.UserId;
                var isAdministrator = await _permissionService.UserHasPermissionAsync(editPostDto, AdministrationPermissions.Post);

                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                post.MessageBody = editPostDto.MessageBody;
                post.PictureId = editPostDto.PictureId;
                post.LastEdit = DateTime.UtcNow;

                await _uow.SaveChangesAsync(editPostDto.UserId);
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        public async Task<string> GetPostBodyAsync(int postId)
        {
            return (await _postsDbSet.FirstOrDefaultAsync(p => p.Id == postId))?.MessageBody;
        }

        public async Task DeleteWallPostAsync(int postId, UserAndOrganizationDto userOrg)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var post = await _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefaultAsync(s =>
                        s.Id == postId &&
                        s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = await _moderatorsDbSet
                    .AnyAsync(x => x.UserId == userOrg.UserId && x.WallId == post.WallId);

                var isAdministrator = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                await _commentService.DeleteCommentsByPostAsync(post.Id);
                _postsDbSet.Remove(post);

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        public async Task HideWallPostAsync(int postId, UserAndOrganizationDto userOrg)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var post = await _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefaultAsync(s => s.Id == postId && s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = await _moderatorsDbSet
                    .AnyAsync(x => x.UserId == userOrg.UserId && x.WallId == post.WallId) || post.AuthorId == userOrg.UserId;

                var isAdministrator = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                post.IsHidden = true;
                post.LastEdit = DateTime.UtcNow;

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        private static UserDto MapUserToDto(ApplicationUser user)
        {
            var userDto = new UserDto
            {
                UserId = user.Id,
                // ReSharper disable once HeapView.BoxingAllocation (Don't change to string interpolation, since EF won't handle project it to SQL)
                FullName = user.FirstName + ' ' + user.LastName,
                PictureId = user.PictureId
            };

            return userDto;
        }

        private static NewlyCreatedPostDto MapNewlyCreatedPostToDto(Post post, UserDto user, WallType wallType, IEnumerable<string> mentionedUserIds)
        {
            var newlyCreatedPostDto = new NewlyCreatedPostDto
            {
                Id = post.Id,
                MessageBody = post.MessageBody,
                PictureId = post.PictureId,
                Created = post.Created,
                CreatedBy = post.CreatedBy,
                User = user,
                WallType = wallType,
                WallId = post.WallId,
                MentionedUsersIds = mentionedUserIds
            };

            return newlyCreatedPostDto;
        }

        public async Task ToggleWatchAsync(int postId, UserAndOrganizationDto userAndOrg, bool shouldWatch)
        {
            await _postDeleteLock.WaitAsync();

            try
            {
                var entity = await _postWatchers.FindAsync(postId, userAndOrg.UserId);

                if (shouldWatch && entity == null)
                {
                    entity = new PostWatcher
                    {
                        PostId = postId,
                        UserId = userAndOrg.UserId
                    };
                    _postWatchers.Add(entity);
                }

                if (!shouldWatch && entity != null)
                {
                    _postWatchers.Remove(entity);
                }

                await _uow.SaveChangesAsync();
            }
            finally
            {
                _postDeleteLock.Release();
            }
        }

        public async Task<IList<string>> GetPostWatchersForAppNotificationsAsync(int postId)
        {
            return await _postWatchers
                .Where(w => w.PostId == postId && (w.User.NotificationsSettings == null || w.User.NotificationsSettings.FollowingPostsAppNotifications))
                .Select(s => s.UserId)
                .ToListAsync();
        }

        public async Task<IList<ApplicationUser>> GetPostWatchersForEmailNotificationsAsync(int postId)
        {
            return await _postWatchers
                .Where(w => w.PostId == postId && (w.User.NotificationsSettings == null || w.User.NotificationsSettings.FollowingPostsEmailNotifications))
                .Include(w => w.User)
                .Where(w => w.User != null)
                .Select(w => w.User)
                .ToListAsync();
        }
    }
}
