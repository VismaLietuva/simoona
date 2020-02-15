using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
        private static object _postDeleteLock = new object();

        private readonly IPermissionService _permissionService;
        private readonly ICommentService _commentService;

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallModerator> _moderatorsDbSet;
        private readonly IDbSet<DataLayer.EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly IDbSet<PostWatcher> _postWatchers;

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

        public NewlyCreatedPostDTO CreateNewPost(NewPostDTO newPostDto)
        {
            lock (_postDeleteLock)
            {
                var wall = _wallsDbSet.FirstOrDefault(x => x.Id == newPostDto.WallId && x.OrganizationId == newPostDto.OrganizationId);

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
                _uow.SaveChanges(newPostDto.UserId);
                _postWatchers.Add(new PostWatcher
                {
                    PostId = post.Id,
                    UserId = newPostDto.UserId
                });
                _uow.SaveChanges(newPostDto.UserId);

                var postCreator = _usersDbSet.Single(user => user.Id == newPostDto.UserId);
                var postCreatorDto = MapUserToDto(postCreator);
                var newlyCreatedPostDto = MapNewlyCreatedPostToDto(post, postCreatorDto, wall.Type);

                return newlyCreatedPostDto;
            }
        }

        public void ToggleLike(int postId, UserAndOrganizationDTO userOrg)
        {
            lock (_postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(x => x.Id == postId && x.Wall.OrganizationId == userOrg.OrganizationId);

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

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        public void EditPost(EditPostDTO editPostDto)
        {
            lock (_postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(x =>
                        x.Id == editPostDto.Id &&
                        x.Wall.OrganizationId == editPostDto.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = _moderatorsDbSet.Any(x => x.UserId == editPostDto.UserId && x.WallId == post.WallId) || post.CreatedBy == editPostDto.UserId;
                var isAdministrator = _permissionService.UserHasPermission(editPostDto, AdministrationPermissions.Post);

                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                post.MessageBody = editPostDto.MessageBody;
                post.PictureId = editPostDto.PictureId;
                post.LastEdit = DateTime.UtcNow;

                _uow.SaveChanges(editPostDto.UserId);
            }
        }

        public void DeleteWallPost(int postId, UserAndOrganizationDTO userOrg)
        {
            lock (_postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(s =>
                        s.Id == postId &&
                        s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = _moderatorsDbSet
                    .Any(x => x.UserId == userOrg.UserId && x.WallId == post.WallId);

                var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                _commentService.DeleteCommentsByPost(post.Id);
                _postsDbSet.Remove(post);

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        public void HideWallPost(int postId, UserAndOrganizationDTO userOrg)
        {
            lock (_postDeleteLock)
            {
                var post = _postsDbSet
                    .Include(x => x.Wall)
                    .FirstOrDefault(s =>
                        s.Id == postId &&
                        s.Wall.OrganizationId == userOrg.OrganizationId);

                if (post == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
                }

                var isWallModerator = _moderatorsDbSet
                                          .Any(x => x.UserId == userOrg.UserId && x.WallId == post.WallId) || post.AuthorId == userOrg.UserId;

                var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Post);
                if (!isAdministrator && !isWallModerator)
                {
                    throw new UnauthorizedException();
                }

                post.IsHidden = true;
                post.LastEdit = DateTime.UtcNow;

                _uow.SaveChanges(userOrg.UserId);
            }
        }

        private UserDto MapUserToDto(ApplicationUser user)
        {
            var userDto = new UserDto
            {
                UserId = user.Id,
                FullName = user.FirstName + ' ' + user.LastName,
                PictureId = user.PictureId,
            };
            return userDto;
        }

        private NewlyCreatedPostDTO MapNewlyCreatedPostToDto(Post post, UserDto user, WallType wallType)
        {
            var newlyCreatedPostDto = new NewlyCreatedPostDTO
            {
                Id = post.Id,
                MessageBody = post.MessageBody,
                PictureId = post.PictureId,
                Created = post.Created,
                CreatedBy = post.CreatedBy,
                User = user,
                WallType = wallType,
                WallId = post.WallId
            };
            return newlyCreatedPostDto;
        }

        public void ToggleWatch(int postId, UserAndOrganizationDTO userAndOrg, bool shouldWatch)
        {
            lock (_postDeleteLock)
            {
                var entity = _postWatchers.Find(postId, userAndOrg.UserId);
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

                _uow.SaveChanges();
            }
        }

        public IEnumerable<string> GetPostWatchersForAppNotifications(int postId)
        {
            return _postWatchers
                .Where(w => w.PostId == postId && (w.User.NotificationsSettings == null || w.User.NotificationsSettings.FollowingPostsAppNotifications))
                .Select(s => s.UserId).ToList()
                .Select(s => s.ToString());
        }

        public IEnumerable<ApplicationUser> GetPostWatchersForEmailNotifications(int postId)
        {
            return _postWatchers
                .Where(w => w.PostId == postId && (w.User.NotificationsSettings == null || w.User.NotificationsSettings.FollowingPostsEmailNotifications))
                .Include(w => w.User)
                .Where(w => w.User != null)
                .Select(w => w.User).ToList();
        }
    }
}