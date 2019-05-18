using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Moderator;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Permissions;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Wall
{
    public class WallService : IWallService
    {
        private static object joinWallLock = new object();

        private readonly IMapper _mapper;
        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;

        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<WallMember> _wallUsersDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallModerator> _moderatorsDbSet;
        private readonly IDbSet<EntityModels.Models.Multiwall.Wall> _wallsDbSet;

        public WallService(
            IMapper mapper,
            IUnitOfWork2 uow,
            IPermissionService permissionService)
        {
            _uow = uow;
            _mapper = mapper;
            _permissionService = permissionService;

            _postsDbSet = uow.GetDbSet<Post>();
            _wallUsersDbSet = uow.GetDbSet<WallMember>();
            _moderatorsDbSet = uow.GetDbSet<WallModerator>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallsDbSet = uow.GetDbSet<EntityModels.Models.Multiwall.Wall>();
        }

        public async Task<int> CreateNewWall(CreateWallDto newWallDto)
        {
            var alreadyExists = await _wallsDbSet
                .AnyAsync(w =>
                    w.OrganizationId == newWallDto.OrganizationId &&
                    w.Name == newWallDto.Name &&
                        (w.Type == WallType.UserCreated ||
                         w.Type == WallType.Main));

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.WallNameAlreadyExists, "Wall name already exists");
            }

            if (newWallDto.MembersIds == null || newWallDto.MembersIds.Any())
            {
                newWallDto.MembersIds = newWallDto.ModeratorsIds;
            }
            else
            {
                newWallDto.MembersIds = newWallDto.MembersIds.Union(newWallDto.ModeratorsIds);
            }

            var wall = new EntityModels.Models.Multiwall.Wall
            {
                Access = newWallDto.Access,
                Type = newWallDto.Type == WallType.Main ? WallType.UserCreated : newWallDto.Type,
                OrganizationId = newWallDto.OrganizationId,
                Name = newWallDto.Name,
                Logo = newWallDto.Logo,
                Description = newWallDto.Description,
                Moderators = newWallDto.ModeratorsIds.Select(x => new WallModerator
                {
                    UserId = x,
                }).ToList(),
                Members = newWallDto.MembersIds.Select(x => new WallMember
                {
                    UserId = x,
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true
                }).ToList()
            };

            _wallsDbSet.Add(wall);
            await _uow.SaveChangesAsync(newWallDto.UserId);

            return wall.Id;
        }

        public void UpdateWall(UpdateWallDto updateWallDto)
        {
            var wall = _wallsDbSet
                .Include(w => w.Moderators)
                .Include(w => w.Members)
                .FirstOrDefault(w =>
                    w.Id == updateWallDto.Id &&
                    w.OrganizationId == updateWallDto.OrganizationId);

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
            }

            var isWallAdmin = _permissionService.UserHasPermission(updateWallDto, AdministrationPermissions.Wall);
            var isModerator = wall.Moderators.Any(m => m.UserId == updateWallDto.UserId);

            if (!isWallAdmin && !isModerator)
            {
                throw new UnauthorizedException();
            }

            IEnumerable<string> newMembersIds = new List<string>();
            IEnumerable<string> newModeratorsIds = new List<string>();
            ICollection<WallModerator> removedModerators = new List<WallModerator>();

            if (updateWallDto.ModeratorsIds != null)
            {
                newMembersIds = updateWallDto.ModeratorsIds.Where(x => !wall.Members.Select(m => m.UserId).Contains(x));
                newModeratorsIds = updateWallDto.ModeratorsIds.Where(x => !wall.Moderators.Select(m => m.UserId).Contains(x));
                removedModerators = wall.Moderators.Where(x => !updateWallDto.ModeratorsIds.Contains(x.UserId)).ToList();
            }

            wall.Description = updateWallDto.Description;
            wall.Logo = updateWallDto.Logo;
            wall.Name = updateWallDto.Name;

            foreach (var newMemberId in newMembersIds)
            {
                wall.Members.Add(new WallMember { UserId = newMemberId });
            }

            foreach (var newModeratorId in newModeratorsIds)
            {
                wall.Moderators.Add(new WallModerator { UserId = newModeratorId });
            }

            foreach (var removedModerator in removedModerators)
            {
                _moderatorsDbSet.Remove(removedModerator);
                wall.Moderators.Remove(removedModerator);
            }

            _uow.SaveChanges(updateWallDto.UserId);
        }

        public async Task<WallDto> WallDetails(int wallId, UserAndOrganizationDTO userOrg)
        {
            var wall = await _wallsDbSet
                .Include(w => w.Members)
                .Where(x => x.Id == wallId && x.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Type == WallType.Main || x.Type == WallType.UserCreated)
                .Select(x => new WallDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type,
                    IsFollowing = x.Members.Any(m => m.UserId == userOrg.UserId),
                    Logo = x.Logo,
                    TotalMembers = x.Members.Count
                })
                .SingleOrDefaultAsync();

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist);
            }

            await MapModeratorsToWalls(new List<WallDto> { wall });
            wall.IsWallModerator = wall.Moderators.Any(m => m.Id == userOrg.UserId);

            return wall;
        }

        public async Task<IEnumerable<WallDto>> GetWallsList(UserAndOrganizationDTO userOrg, WallsListFilter filter)
        {
            var walls = filter == WallsListFilter.Followed ?
                await GetUserFollowedWalls(userOrg) :
                await GetAllOrNotFollowedWalls(userOrg, filter);

            await MapModeratorsToWalls(walls);

            return walls;
        }

        public async Task<IEnumerable<PostDTO>> GetWallPosts(int pageNumber, int pageSize, UserAndOrganizationDTO userOrg, int? wallId)
        {
            return await QueryForPosts(userOrg, pageNumber, pageSize, wallId, null);
        }

        public async Task<IEnumerable<PostDTO>> GetAllPosts(int pageNumber, int pageSize, UserAndOrganizationDTO userOrg)
        {
            return await QueryForPosts(userOrg, pageNumber, pageSize, null, null);
        }

        public async Task<PostDTO> GetWallPost(UserAndOrganizationDTO userOrg, int postId)
        {
            var post = await _postsDbSet
                .Include(p => p.Wall)
                .Include(p => p.Comments)
                .Include(p => p.Wall)
                .FirstOrDefaultAsync(p =>
                    p.Wall.OrganizationId == userOrg.OrganizationId &&
                    p.Id == postId);

            if (post == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Post not found");
            }

            var moderators = await _moderatorsDbSet.Where(x => x.WallId == post.WallId).ToListAsync();
            var postInList = new List<Post>() { post };
            var users = await GetUsers(postInList);
            var posts = MapPostsWithChildEntitiesToDto(userOrg.UserId, postInList, users, moderators)
                .FirstOrDefault();
            return posts;
        }

        public async Task<IEnumerable<PostDTO>> SearchWall(string searchString, UserAndOrganizationDTO userOrg, int pageNumber, int pageSize)
        {
            Expression<Func<Post, bool>> exp = post =>
                post.MessageBody.Contains(searchString) ||
                post.Comments.Any(comment =>
                    comment.MessageBody.Contains(searchString) &&
                    comment.AuthorId != null);

            return await QueryForPosts(userOrg, pageNumber, pageSize, null, exp);
        }

        public async Task<IEnumerable<WallMemberDto>> GetWallMembers(int wallId, UserAndOrganizationDTO userOrg)
        {
            var wallExists = await _wallsDbSet
                .AnyAsync(wall =>
                    wall.Id == wallId &&
                    wall.OrganizationId == userOrg.OrganizationId);

            if (!wallExists)
            {
                throw new ValidationException(ErrorCodes.WallNotFound, "Wall not found");
            }

            var wallMembers = await _wallUsersDbSet
                .Include(u => u.Wall.Moderators)
                .Include(u => u.User.JobPosition)
                .Where(u =>
                    u.WallId == wallId &&
                    u.Wall.OrganizationId == userOrg.OrganizationId)
                .Select(u => new WallMemberDto
                {
                    Id = u.UserId,
                    JobTitle = u.User.JobPosition.Title ?? string.Empty,
                    IsModerator = u.Wall.Moderators.Any(m => m.UserId == u.UserId),
                    FullName = u.User.FirstName + " " + u.User.LastName,
                    ProfilePicture = u.User.PictureId,
                    IsCurrentUser = u.UserId == userOrg.UserId
                })
                .OrderBy(m => m.FullName)
                .ToListAsync();

            return wallMembers;
        }

        public IEnumerable<string> GetWallMembersIds(int wallId, UserAndOrganizationDTO userOrg)
        {
            var wallExists = _wallsDbSet
                .Any(wall =>
                    wall.Id == wallId &&
                    wall.OrganizationId == userOrg.OrganizationId);

            if (!wallExists)
            {
                throw new ValidationException(ErrorCodes.WallNotFound, "Wall not found");
            }

            var wallMembers = _wallUsersDbSet
                .Include(u => u.Wall)
                .Include(u => u.User)
                .Where(u =>
                    u.WallId == wallId &&
                    u.Wall.OrganizationId == userOrg.OrganizationId)
                .Where(u => u.UserId != userOrg.UserId)
                .Select(u => u.UserId)
                .ToList();

            return wallMembers;
        }

        public ApplicationUserMinimalViewModelDto JoinLeaveWall(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall)
        {
            lock (joinWallLock)
            {
                var wallTypeFilter = isEventWall
                    ? (Expression<Func<EntityModels.Models.Multiwall.Wall, bool>>)(w => w.Type == WallType.Events)
                    : (w => w.Type != WallType.Events);

                var wall = _wallsDbSet
                    .Include(w => w.Members)
                    .Include(w => w.Moderators)
                    .Where(wallTypeFilter)
                    .FirstOrDefault(w =>
                        w.Id == wallId &&
                        w.OrganizationId == tenantId);

                if (wall == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "wall does not exist");
                }

                if (attendeeId != actorId)
                {
                    var isModerator = wall.Moderators
                        .Any(m => m.UserId == actorId);
                    var isWallAdmin =
                        _permissionService.UserHasPermission(
                            new UserAndOrganizationDTO { OrganizationId = tenantId, UserId = actorId },
                            AdministrationPermissions.Wall);

                    if (isModerator || isWallAdmin)
                    {
                        var attendingUserExists = _usersDbSet
                        .Any(u => u.Id == attendeeId && u.OrganizationId == tenantId);
                        if (!attendingUserExists)
                        {
                            throw new ValidationException(ErrorCodes.ContentDoesNotExist, "user not found");
                        }
                    }
                    else
                    {
                        throw new UnauthorizedException();
                    }
                }

                if (wall.Members.Any(u => u.UserId == attendeeId))
                {
                    LeaveWall(attendeeId, wall);
                }
                else
                {
                    JoinWall(attendeeId, wall);
                }

                _uow.SaveChanges(actorId);

                var userInfo = _usersDbSet
                    .Include(u => u.JobPosition)
                    .Where(u => u.Id == attendeeId && u.OrganizationId == tenantId)
                    .Select(u => new ApplicationUserMinimalViewModelDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PictureId = u.PictureId,
                        JobPosition = u.JobPosition.Title ?? string.Empty
                    })
                    .First();

                return userInfo;
            }
        }

        public void DeleteWall(int wallId, UserAndOrganizationDTO userOrg, WallType type)
        {
            var wall = _wallsDbSet
                .Include(x => x.Moderators)
                .Include(x => x.Members)
                .Include(x => x.Posts)
                .Include(x => x.Posts.Select(y => y.Comments))
                .FirstOrDefault(x => x.Id == wallId &&
                    x.OrganizationId == userOrg.OrganizationId &&
                    x.Type == type);

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist);
            }

            var hasPermission = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Wall);

            var isWallModerator = wall.Moderators.Any(x => x.UserId == userOrg.UserId) || hasPermission;
            if (!isWallModerator)
            {
                throw new UnauthorizedException();
            }

            _wallsDbSet.Remove(wall);
            _uow.SaveChanges(userOrg.UserId);
        }

        public void RemoveModerator(int wallId, string responsibleUserId, UserAndOrganizationDTO userAndOrg)
        {
            var wall = _wallsDbSet
                .Include(x => x.Moderators)
                .Single(x => x.Id == wallId &&
                    x.OrganizationId == userAndOrg.OrganizationId);

            var moderator = wall.Moderators.SingleOrDefault(x => x.UserId == responsibleUserId);
            if (moderator == null)
            {
                return;
            }

            _moderatorsDbSet.Remove(moderator);
            _uow.SaveChanges(userAndOrg.UserId);
        }

        public void AddModerator(int wallId, string responsibleUserId, UserAndOrganizationDTO userId)
        {
            var wall = _wallsDbSet
                .Include(x => x.Moderators)
                .Single(x => x.Id == wallId &&
                    x.OrganizationId == userId.OrganizationId);

            if (wall.Moderators.Any(x => x.UserId == responsibleUserId))
            {
                return;
            }

            var newModerator = new WallModerator
            {
                WallId = wallId,
                UserId = responsibleUserId
            };

            AddMemberToWalls(responsibleUserId, new List<int> { wallId });

            _moderatorsDbSet.Add(newModerator);
            _uow.SaveChanges(userId.UserId);
        }

        public void AddMemberToWalls(string userId, List<int> wallIds)
        {
            if (wallIds.Any())
            {
                var wallMembers = _wallUsersDbSet
                    .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                    .ToList();

                foreach (var wallId in wallIds)
                {
                    if (!wallMembers.Any(x => x.WallId == wallId))
                    {
                        var wallMember = new WallMember()
                        {
                            AppNotificationsEnabled = true,
                            EmailNotificationsEnabled = true,
                            UserId = userId,
                            WallId = wallId,
                        };

                        _wallUsersDbSet.Add(wallMember);
                    }
                }
            }
        }

        public void RemoveMemberFromWall(string userId, int wallId)
        {
            RemoveMemberFromWalls(userId, new List<int> { wallId });
        }

        public void RemoveMemberFromWalls(string userId, List<int> wallIds)
        {
            if (wallIds.Any())
            {
                var wallModerators = _moderatorsDbSet
                    .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                    .ToList();

                var wallMembers = _wallUsersDbSet
                    .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                    .ToList();

                foreach (var member in wallMembers)
                {
                    if (!wallModerators.Any(x => x.UserId == member.UserId && x.WallId == member.WallId))
                    {
                        wallMembers.ForEach(x => _wallUsersDbSet.Remove(x));
                    }
                }
            }
        }

        public void ReplaceMembersInWall(IEnumerable<ApplicationUser> newMembers, int wallId, string currentUserId)
        {
            var wallModerators = _moderatorsDbSet
                .Where(x => x.WallId == wallId)
                .ToList();

            var currentWallMembers = _wallUsersDbSet.Where(x => x.WallId == wallId).ToList();
            var membersToRemove = currentWallMembers.Where(x => !newMembers.Select(y => y.Id).Contains(x.UserId) &&
                                                                !wallModerators.Select(y => y.UserId).Contains(x.UserId));

            foreach (var member in membersToRemove)
            {
                _wallUsersDbSet.Remove(member);
            }

            var membersToAdd = newMembers.Where(x => !currentWallMembers.Select(y => y.UserId).Contains(x.Id));

            foreach (var member in membersToAdd)
            {
                var newMember = new WallMember()
                {
                    UserId = member.Id,
                    WallId = wallId,
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true
                };
                _wallUsersDbSet.Add(newMember);
            }

            _uow.SaveChanges(currentUserId);
        }

        private async Task MapModeratorsToWalls(IEnumerable<WallDto> walls)
        {
            var wallsIds = walls.Select(w => w.Id).ToList();

            var moderators = await _moderatorsDbSet
                .Include(m => m.User)
                .Where(m => wallsIds.Contains(m.WallId))
                .Select(m => new
                {
                    Id = m.User.Id,
                    WallId = m.WallId,
                    Fullname = string.IsNullOrEmpty(m.User.FirstName) && string.IsNullOrEmpty(m.User.LastName) ?
                        string.Empty :
                        m.User.FirstName + " " + m.User.LastName
                })
                .ToListAsync();

            foreach (var wall in walls)
            {
                wall.Moderators = moderators
                    .Where(m => m.WallId == wall.Id)
                    .Select(m => new ModeratorDto
                    {
                        Id = m.Id,
                        Fullname = m.Fullname
                    })
                    .ToList();
            }
        }

        private void JoinWall(string userId, EntityModels.Models.Multiwall.Wall wall)
        {
            var newMember = new WallMember
            {
                UserId = userId,
                WallId = wall.Id,
                AppNotificationsEnabled = true,
                EmailNotificationsEnabled = true
            };

            wall.Members.Add(newMember);
        }

        private void LeaveWall(string userId, EntityModels.Models.Multiwall.Wall wall)
        {
            var isModerator = wall.Moderators
                .Any(m => m.UserId == userId && m.WallId == wall.Id);

            if (isModerator && wall.Type == WallType.UserCreated)
            {
                throw new ValidationException(ErrorCodes.WallModeratorCanNotLeave, "Moderator can not leave wall");
            }

            if (wall.Type == WallType.Main)
            {
                throw new ValidationException(ErrorCodes.WallCannotLeaveMain, "Can not leave main wall");
            }

            var member = wall.Members.First(x => x.UserId == userId);
            _wallUsersDbSet.Remove(member);
        }

        private async Task<IEnumerable<PostDTO>> QueryForPosts(UserAndOrganizationDTO userOrg, int pageNumber, int pageSize, int? wallId, Expression<Func<Post, bool>> filter)
        {
            if (filter == null)
            {
                filter = post => true;
            }

            var wallsIds = wallId.HasValue && WallIsValid(userOrg, wallId.Value) ?
                new List<int> { wallId.Value } :
                (await GetWallsList(userOrg, WallsListFilter.Followed))
                    .Select(w => w.Id).ToList();

            int entriesCountToSkip = (pageNumber - 1) * pageSize;
            var posts = await _postsDbSet
                .Include(post => post.Wall)
                .Include(post => post.Comments)
                .Where(post => wallsIds.Contains(post.WallId))
                .Where(filter)
                .OrderByDescending(x => x.LastActivity)
                .Skip(() => entriesCountToSkip)
                .Take(() => pageSize)
                .ToListAsync();

            IEnumerable<WallModerator> moderators = await _moderatorsDbSet.Where(x => wallsIds.Contains(x.WallId)).ToListAsync();
            IEnumerable<ApplicationUser> users = await GetUsers(posts);

            return MapPostsWithChildEntitiesToDto(userOrg.UserId, posts, users, moderators);
        }

        private async Task<IEnumerable<ApplicationUser>> GetUsers(IEnumerable<Post> posts)
        {
            var comments = posts.SelectMany(s => s.Comments).ToList();

            var usersIdsFromLikes = posts
                .Select(x => x.Likes)
                .Concat(comments.Select(x => x.Likes))
                .SelectMany(l => l.Select(x => x.UserId));

            var postUserIds = posts.Select(user => user.AuthorId);
            var commentUserIds = comments.Select(user => user.AuthorId);

            var userIds = postUserIds
                .Union(commentUserIds)
                .Union(usersIdsFromLikes)
                .Distinct()
                .ToList();

            return await _usersDbSet.Where(user => userIds.Contains(user.Id)).ToListAsync();
        }

        private IEnumerable<PostDTO> MapPostsWithChildEntitiesToDto(string userId, IEnumerable<Post> posts, IEnumerable<ApplicationUser> users, IEnumerable<WallModerator> moderators)
        {
            var results = new List<PostDTO>();
            foreach (var post in posts)
            {
                var postDto = _mapper.Map<PostDTO>(post);

                if (postDto.IsHidden)
                {
                    postDto.MessageBody = string.Empty;
                    postDto.PictureId = string.Empty;
                }

                postDto.IsEdited = post.LastEdit > post.Created;
                postDto.Author = MapUserToDto(post.AuthorId, users);
                postDto.Likes = MapLikesToDto(post.Likes, users);
                postDto.IsLiked = post.IsLikedByUser(userId);
                postDto.CanModerate = moderators.Any(x => x.UserId == userId && x.WallId == post.WallId) || post.CreatedBy == userId;
                postDto.SharedEventId = post.SharedEventId;
                postDto.Comments = post.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Likes = MapLikesToDto(c.Likes, users),
                    Author = MapUserToDto(c.AuthorId, users),
                    IsLiked = c.IsLikedByUser(userId),
                    MessageBody = c.IsHidden
                                ? string.Empty
                                : c.MessageBody,

                    PictureId = c.IsHidden
                                ? string.Empty
                                : c.PictureId,

                    Created = c.Created,
                    PostId = c.PostId,
                    CanModerate = moderators.Any(x => x.UserId == userId && x.WallId == c.Post.WallId) || c.AuthorId == userId,
                    IsEdited = c.LastEdit > c.Created,
                    LastEdit = c.LastEdit,
                    IsHidden = c.IsHidden
                });

                results.Add(postDto);
            }

            return results;
        }

        private UserDto MapUserToDto(string userId, IEnumerable<ApplicationUser> users)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);

            return user == null ? new UserDto { FullName = ConstBusinessLayer.DeletedUserName } : _mapper.Map<UserDto>(user);
        }

        private IEnumerable<UserDto> MapLikesToDto(LikesCollection likes, IEnumerable<ApplicationUser> users)
        {
            return likes
                .Select(like => users.FirstOrDefault(x => x.Id == like.UserId))
                .Select(user => _mapper.Map<UserDto>(user))
                .Where(user => user != null)
                .ToList();
        }

        private bool WallIsValid(UserAndOrganizationDTO userOrg, int wallId)
        {
            var wallExists = _wallsDbSet.Any(w => w.Id == wallId && w.OrganizationId == userOrg.OrganizationId);
            if (!wallExists)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
            }

            return true;
        }

        private async Task<IEnumerable<WallDto>> GetAllOrNotFollowedWalls(UserAndOrganizationDTO userOrg, WallsListFilter filter)
        {
            var wallFilters = new Dictionary<WallsListFilter, Expression<Func<EntityModels.Models.Multiwall.Wall, bool>>>
            {
                { WallsListFilter.All, w => true },
                { WallsListFilter.NotFollowed, w => w.Members.All(m => m.UserId != userOrg.UserId) }
            };

            var walls = await _wallsDbSet
                .Include(w => w.Members)
                .Where(w => w.OrganizationId == userOrg.OrganizationId)
                .Where(wallFilters[filter])
                .Where(w => w.Type == WallType.Main || w.Type == WallType.UserCreated)
                .OrderBy(x => x.Type)
                .ThenBy(w => w.Name.ToLower())
                .Select(w => new WallDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    IsFollowing = w.Members.Any(m => m.UserId == userOrg.UserId),
                    Type = w.Type,
                    Logo = w.Logo
                })
                .ToListAsync();
            return walls;
        }

        private async Task<IEnumerable<WallDto>> GetUserFollowedWalls(UserAndOrganizationDTO userOrg)
        {
            var walls = await _wallUsersDbSet
                .Include(x => x.Wall)
                .Where(x => x.UserId == userOrg.UserId && x.Wall.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Wall.Type == WallType.Main || x.Wall.Type == WallType.UserCreated)
                .OrderBy(x => x.Wall.Type)
                .ThenBy(x => x.Wall.Name)
                .Select(x => new WallDto
                {
                    Id = x.Wall.Id,
                    Name = x.Wall.Name,
                    Description = x.Wall.Description,
                    Type = x.Wall.Type,
                    IsFollowing = true,
                    Logo = x.Wall.Logo
                })
                .ToListAsync();
            return walls;
        }
    }
}