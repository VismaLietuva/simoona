using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Moderator;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Wall
{
    public class WallService : IWallService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IMapper _mapper;
        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;

        private readonly IDbSet<Post> _postsDbSet;
        private readonly IDbSet<WallMember> _wallUsersDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallModerator> _moderatorsDbSet;
        private readonly IDbSet<MultiwallWall> _wallsDbSet;
        private readonly IDbSet<PostWatcher> _postWatchers;

        public WallService(IMapper mapper, IUnitOfWork2 uow, IPermissionService permissionService)
        {
            _uow = uow;
            _mapper = mapper;
            _permissionService = permissionService;

            _postsDbSet = uow.GetDbSet<Post>();
            _wallUsersDbSet = uow.GetDbSet<WallMember>();
            _moderatorsDbSet = uow.GetDbSet<WallModerator>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallsDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _postWatchers = uow.GetDbSet<PostWatcher>();
        }

        public async Task<int> CreateNewWallAsync(CreateWallDto newWallDto)
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

            var wall = new MultiwallWall
            {
                Access = newWallDto.Access,
                Type = newWallDto.Type == WallType.Main ? WallType.UserCreated : newWallDto.Type,
                OrganizationId = newWallDto.OrganizationId,
                Name = newWallDto.Name,
                Logo = newWallDto.Logo,
                Description = newWallDto.Description,
                Moderators = newWallDto.ModeratorsIds.Select(x => new WallModerator
                {
                    UserId = x
                }).ToList(),
                IsHiddenFromAllWalls = newWallDto.IsHiddenFromAllWalls,
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

        public async Task UpdateWallAsync(UpdateWallDto updateWallDto)
        {
            var wall = await _wallsDbSet
                .Include(w => w.Moderators)
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w =>
                    w.Id == updateWallDto.Id &&
                    w.OrganizationId == updateWallDto.OrganizationId);

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
            }

            await CheckIfUserIsAllowedToModifyWallContentAsync(wall, updateWallDto);

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
            wall.IsHiddenFromAllWalls = updateWallDto.IsHiddenFromAllWalls;

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

            await _uow.SaveChangesAsync(updateWallDto.UserId);
        }

        public async Task<WallDto> GetWallAsync(int wallId, UserAndOrganizationDto userOrg)
        {
            var wall = await _wallsDbSet
                .Where(x => x.Id == wallId && x.OrganizationId == userOrg.OrganizationId)
                .Select(x => new WallDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type,
                    Logo = x.Logo
                })
                .SingleOrDefaultAsync();

            return wall;
        }

        public async Task<WallDto> GetWallDetailsAsync(int wallId, UserAndOrganizationDto userOrg)
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
                    IsFollowing = x.Type == WallType.Main || x.Members.Any(m => m.UserId == userOrg.UserId),
                    Logo = x.Logo,
                    TotalMembers = x.Members.Count,
                    IsHiddenFromAllWalls = x.IsHiddenFromAllWalls
                })
                .SingleOrDefaultAsync();

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist);
            }

            await MapModeratorsToWallsAsync(new List<WallDto> { wall });
            wall.IsWallModerator = wall.Moderators.Any(m => m.Id == userOrg.UserId);

            return wall;
        }

        public async Task<IEnumerable<WallDto>> GetWallsListAsync(UserAndOrganizationDto userOrg, WallsListFilter filter)
        {
            var walls = filter == WallsListFilter.Followed ? await GetUserFollowedWallsAsync(userOrg) : await GetAllOrNotFollowedWallsAsync(userOrg, filter);

            await MapModeratorsToWallsAsync(walls);

            return walls;
        }

        public async Task<IEnumerable<PostDto>> GetWallPostsAsync(int pageNumber, int pageSize, UserAndOrganizationDto userOrg, int? wallId)
        {
            return await QueryForPostsAsync(userOrg, pageNumber, pageSize, wallId, null, WallsListFilter.None);
        }

        public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int pageNumber, int pageSize, UserAndOrganizationDto userOrg, WallsListFilter filter)
        {
            return await QueryForPostsAsync(userOrg, pageNumber, pageSize, null, null, filter);
        }

        public async Task<PostDto> GetWallPostAsync(UserAndOrganizationDto userOrg, int postId)
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
            var postInList = new List<Post> { post };
            var watchedPosts = await RetrieveWatchedPostsAsync(userOrg.UserId, postInList);
            var users = await GetUsersAsync(postInList);
            var posts = MapPostsWithChildEntitiesToDto(userOrg.UserId, postInList, users, moderators, watchedPosts);

            return posts.FirstOrDefault();
        }

        public async Task<IEnumerable<PostDto>> SearchWallAsync(string searchString, UserAndOrganizationDto userOrg, int pageNumber, int pageSize)
        {
            Expression<Func<Post, bool>> exp = post =>
                (!post.IsHidden && post.MessageBody.Contains(searchString)) ||
                post.Comments.Any(comment =>
                    !comment.IsHidden &&
                    comment.MessageBody.Contains(searchString) &&
                    comment.AuthorId != null);

            return await QueryForPostsAsync(userOrg, pageNumber, pageSize, null, exp, WallsListFilter.All);
        }

        public async Task<IEnumerable<WallMemberDto>> GetWallMembersAsync(int wallId, UserAndOrganizationDto userOrg)
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

        public async Task<IEnumerable<string>> GetWallMembersIdsAsync(int wallId, UserAndOrganizationDto userOrg)
        {
            var wallExists = await _wallsDbSet.AnyAsync(wall =>
                    wall.Id == wallId &&
                    wall.OrganizationId == userOrg.OrganizationId);

            if (!wallExists)
            {
                throw new ValidationException(ErrorCodes.WallNotFound, "Wall not found");
            }

            var wallMembers = await _wallUsersDbSet
                .Include(u => u.Wall)
                .Include(u => u.User)
                .Where(u =>
                    u.WallId == wallId &&
                    u.Wall.OrganizationId == userOrg.OrganizationId)
                .Where(u => u.UserId != userOrg.UserId)
                .Select(u => u.UserId)
                .Distinct()
                .ToListAsync();

            return wallMembers;
        }

        public async Task<ApplicationUserMinimalDto> JoinOrLeaveWallAsync(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var wallTypeFilter = isEventWall
                    ? (Expression<Func<DataLayer.EntityModels.Models.Multiwall.Wall, bool>>)(w => w.Type == WallType.Events)
                    : (w => w.Type != WallType.Events);

                var wall = await _wallsDbSet
                    .Include(w => w.Members)
                    .Include(w => w.Moderators)
                    .Where(wallTypeFilter)
                    .FirstOrDefaultAsync(w => w.Id == wallId && w.OrganizationId == tenantId);

                if (wall == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "wall does not exist");
                }

                if (attendeeId != actorId)
                {
                    var isModerator = wall.Moderators.Any(m => m.UserId == actorId);
                    var isWallAdmin = await _permissionService.UserHasPermissionAsync(new UserAndOrganizationDto { OrganizationId = tenantId, UserId = actorId }, AdministrationPermissions.Wall);

                    if (isModerator || isWallAdmin)
                    {
                        var attendingUserExists = await _usersDbSet.AnyAsync(u => u.Id == attendeeId && u.OrganizationId == tenantId);

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

                await _uow.SaveChangesAsync(actorId);

                var userInfo = await _usersDbSet
                    .Include(u => u.JobPosition)
                    .Where(u => u.Id == attendeeId && u.OrganizationId == tenantId)
                    .Select(u => new ApplicationUserMinimalDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PictureId = u.PictureId,
                        JobPosition = u.JobPosition.Title ?? string.Empty
                    })
                    .FirstAsync();

                return userInfo;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task DeleteWallAsync(int wallId, UserAndOrganizationDto userOrg, WallType type)
        {
            var wall = await _wallsDbSet
                .Include(x => x.Moderators)
                .Include(x => x.Members)
                .Include(x => x.Posts)
                .Include(x => x.Posts.Select(y => y.Comments))
                .FirstOrDefaultAsync(x => x.Id == wallId &&
                                     x.OrganizationId == userOrg.OrganizationId &&
                                     x.Type == type);

            if (wall == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist);
            }

            await CheckIfUserIsAllowedToModifyWallContentAsync(wall, userOrg);

            _wallsDbSet.Remove(wall);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task RemoveModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDto userAndOrg)
        {
            var wall = await _wallsDbSet
                .Include(x => x.Moderators)
                .SingleAsync(x => x.Id == wallId && x.OrganizationId == userAndOrg.OrganizationId);

            var moderator = wall.Moderators.SingleOrDefault(x => x.UserId == responsibleUserId);
            if (moderator == null)
            {
                return;
            }

            _moderatorsDbSet.Remove(moderator);
            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        public async Task AddModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDto userId)
        {
            var wall = await _wallsDbSet
                .Include(x => x.Moderators)
                .SingleAsync(x => x.Id == wallId && x.OrganizationId == userId.OrganizationId);

            if (wall.Moderators.Any(x => x.UserId == responsibleUserId))
            {
                return;
            }

            var newModerator = new WallModerator
            {
                WallId = wallId,
                UserId = responsibleUserId
            };

            await AddMemberToWallsAsync(responsibleUserId, new List<int> { wallId });

            _moderatorsDbSet.Add(newModerator);

            await _uow.SaveChangesAsync(userId.UserId);
        }

        public async Task AddMemberToWallsAsync(string userId, List<int> wallIds)
        {
            if (!wallIds.Any())
            {
                return;
            }

            var wallMembers = await _wallUsersDbSet
                .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                .ToListAsync();

            foreach (var wallId in wallIds)
            {
                if (wallMembers.Any(x => x.WallId == wallId))
                {
                    continue;
                }

                var wallMember = new WallMember
                {
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true,
                    UserId = userId,
                    WallId = wallId
                };

                _wallUsersDbSet.Add(wallMember);
            }
        }

        public async Task RemoveMemberFromWallAsync(string userId, int wallId)
        {
            await RemoveMemberFromWallsAsync(userId, new List<int> { wallId });
        }

        public async Task RemoveMemberFromWallsAsync(string userId, List<int> wallIds)
        {
            if (!wallIds.Any())
            {
                return;
            }

            var wallModerators = await _moderatorsDbSet
                .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                .ToListAsync();

            var wallMembers = await _wallUsersDbSet
                .Where(x => wallIds.Contains(x.WallId) && x.UserId == userId)
                .ToListAsync();

            foreach (var member in wallMembers)
            {
                if (!wallModerators.Any(x => x.UserId == member.UserId && x.WallId == member.WallId))
                {
                    wallMembers.ForEach(x => _wallUsersDbSet.Remove(x));
                }
            }
        }

        public async Task ReplaceMembersInWallAsync(IEnumerable<ApplicationUser> newMembers, int wallId, string currentUserId)
        {
            var wallModerators = await _moderatorsDbSet
                .Where(x => x.WallId == wallId)
                .ToListAsync();

            var currentWallMembers = await _wallUsersDbSet.Where(x => x.WallId == wallId).ToListAsync();
            var membersToRemove = currentWallMembers.Where(x => !newMembers.Select(y => y.Id).Contains(x.UserId) &&
                                                                !wallModerators.Select(y => y.UserId).Contains(x.UserId));

            foreach (var member in membersToRemove)
            {
                _wallUsersDbSet.Remove(member);
            }

            var membersToAdd = newMembers.Where(x => !currentWallMembers.Select(y => y.UserId).Contains(x.Id));

            foreach (var member in membersToAdd)
            {
                var newMember = new WallMember
                {
                    UserId = member.Id,
                    WallId = wallId,
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true
                };

                _wallUsersDbSet.Add(newMember);
            }

            await _uow.SaveChangesAsync(currentUserId);
        }

        private async Task MapModeratorsToWallsAsync(IList<WallDto> walls)
        {
            var wallsIds = walls.Select(w => w.Id).ToList();

            var moderators = await _moderatorsDbSet
                .Include(m => m.User)
                .Where(m => wallsIds.Contains(m.WallId))
                .Select(m => new
                {
                    Id = m.User.Id,
                    WallId = m.WallId,
                    Fullname = string.IsNullOrEmpty(m.User.FirstName) && string.IsNullOrEmpty(m.User.LastName) ? string.Empty : m.User.FirstName + " " + m.User.LastName
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

        public async Task<IEnumerable<WallMemberEmailReceiverDto>> GetWallMembersWithEnabledEmailNotificationsAsync(int wallId, int organizationId)
        {
            return await _wallUsersDbSet
                .Where(member =>
                    member.WallId == wallId &&
                    member.EmailNotificationsEnabled)
                .Select(member => new WallMemberEmailReceiverDto
                {
                    Id = member.UserId,
                    Email = member.User.Email,
                    TimeZoneKey = member.User.TimeZone
                })
                .ToListAsync();
        }

        public async Task CheckIfUserIsAllowedToModifyWallContentAsync(
            MultiwallWall wall,
            string createdBy,
            string permission,
            UserAndOrganizationDto userOrg,
            bool checkForAdministrationEventPermission = true)
        {
            var isModerator = createdBy == userOrg.UserId || await _moderatorsDbSet
                .AnyAsync(moderator =>
                    moderator.UserId == userOrg.UserId &&
                    moderator.WallId == wall.Id);

            await CheckIfUserIsAllowedToModifyWallContentAsync(
                wall,
                isModerator,
                permission,
                userOrg,
                checkForAdministrationEventPermission);
        }

        private async Task CheckIfUserIsAllowedToModifyWallContentAsync(MultiwallWall wall, UserAndOrganizationDto userOrg)
        {
            var isModerator = wall.CreatedBy == userOrg.UserId || wall.Moderators.Any(m => m.UserId == userOrg.UserId);

            await CheckIfUserIsAllowedToModifyWallContentAsync(wall, isModerator, AdministrationPermissions.Wall, userOrg, true);
        }

        private async Task CheckIfUserIsAllowedToModifyWallContentAsync(
            MultiwallWall wall,
            bool isModerator,
            string permission,
            UserAndOrganizationDto userOrg,
            bool checkForAdministrationEventPermission)
        {
            if (isModerator)
            {
                return;
            }

            var eventPermission = checkForAdministrationEventPermission ?
                AdministrationPermissions.Event :
                BasicPermissions.Event;

            var hasRequiredPermission = wall.Type != WallType.Events ?
                await _permissionService.UserHasPermissionAsync(userOrg, permission) :
                await _permissionService.UserHasPermissionAsync(userOrg, eventPermission);

            if (!hasRequiredPermission)
            {
                throw new UnauthorizedException();
            }
        }

        private static void JoinWall(string userId, MultiwallWall wall)
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

        private void LeaveWall(string userId, MultiwallWall wall)
        {
            var isModerator = wall.Moderators.Any(m => m.UserId == userId && m.WallId == wall.Id);

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

        private async Task<IEnumerable<PostDto>> QueryForPostsAsync(UserAndOrganizationDto userOrg, int pageNumber, int pageSize, int? wallId, Expression<Func<Post, bool>> filter, WallsListFilter wallsListFilter)
        {
            if (filter == null)
            {
                filter = post => true;
            }

            List<int> wallsIds;

            if (wallId.HasValue && WallIsValid(userOrg, wallId.Value))
            {
                wallsIds = new List<int> { wallId.Value };
            }
            else
            {
                wallsIds = (await GetWallsListAsync(userOrg, wallsListFilter)).Select(w => w.Id).ToList();
            }

            var entriesCountToSkip = (pageNumber - 1) * pageSize;

            var posts = await _postsDbSet
                .Include(post => post.Wall)
                .Include(post => post.Comments)
                .Where(post => wallsIds.Contains(post.WallId))
                .Where(filter)
                .OrderByDescending(x => x.LastActivity)
                .Skip(() => entriesCountToSkip)
                .Take(() => pageSize)
                .ToListAsync();

            var moderators = await _moderatorsDbSet.Where(x => wallsIds.Contains(x.WallId)).ToListAsync();
            var watchedPosts = await RetrieveWatchedPostsAsync(userOrg.UserId, posts);
            var users = await GetUsersAsync(posts);

            return MapPostsWithChildEntitiesToDto(userOrg.UserId, posts, users, moderators, watchedPosts);
        }

        private async Task<List<ApplicationUser>> GetUsersAsync(IReadOnlyCollection<Post> posts)
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

        private IEnumerable<PostDto> MapPostsWithChildEntitiesToDto(string userId, List<Post> posts, List<ApplicationUser> users, List<WallModerator> moderators, HashSet<int> watchedPosts)
        {
            foreach (var post in posts)
            {
                var postDto = _mapper.Map<PostDto>(post);

                if (postDto.IsHidden)
                {
                    postDto.MessageBody = string.Empty;
                    postDto.Images = null;
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
                    Images = c.IsHidden
                        ? null
                        : c.Images,
                    Created = c.Created,
                    PostId = c.PostId,
                    CanModerate = moderators.Any(x => x.UserId == userId && x.WallId == c.Post.WallId) || c.AuthorId == userId,
                    IsEdited = c.LastEdit > c.Created,
                    LastEdit = c.LastEdit,
                    IsHidden = c.IsHidden
                });
                postDto.IsWatched = watchedPosts.Contains(post.Id);

                yield return postDto;
            }
        }

        private async Task<HashSet<int>> RetrieveWatchedPostsAsync(string userId, List<Post> posts)
        {
            var postIds = posts.Select(s => s.Id).ToList();
            var watchedList = await _postWatchers.Where(w => w.UserId == userId && postIds.Contains(w.PostId)).Select(s => s.PostId).ToListAsync();

            return new HashSet<int>(watchedList);
        }

        private UserDto MapUserToDto(string userId, IEnumerable<ApplicationUser> users)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);

            return user == null ? new UserDto { FullName = BusinessLayerConstants.DeletedUserName } : _mapper.Map<UserDto>(user);
        }

        private IEnumerable<LikeDto> MapLikesToDto(LikesCollection likes, IEnumerable<ApplicationUser> users)
        {
            return likes
                .Select(like => new { User = users.FirstOrDefault(user => user.Id == like.UserId), Like = like })
                .Select(likeWithUserData =>
                {
                    if (likeWithUserData.User == null)
                    {
                        return null;
                    }

                    return new LikeDto
                    {
                        UserId = likeWithUserData.User.Id,
                        FullName = likeWithUserData.User.FullName,
                        PictureId = likeWithUserData.User.PictureId,
                        Type = likeWithUserData.Like.Type
                    };
                })
                .Where(like => like != null)
                .ToList();
        }

        private bool WallIsValid(UserAndOrganizationDto userOrg, int wallId)
        {
            var wallExists = _wallsDbSet.Any(w => w.Id == wallId && w.OrganizationId == userOrg.OrganizationId);
            if (!wallExists)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");
            }

            return true;
        }

        private async Task<IList<WallDto>> GetAllOrNotFollowedWallsAsync(UserAndOrganizationDto userOrg, WallsListFilter filter)
        {
            var wallFilters = new Dictionary<WallsListFilter, Expression<Func<MultiwallWall, bool>>>
            {
                { WallsListFilter.All, w => true },
                { WallsListFilter.NotHiddenFromAllWalls, w => !w.IsHiddenFromAllWalls || w.Members.Any(m => m.UserId == userOrg.UserId) },
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
                    // Don't simplify, since it's EF projection
                    IsFollowing = w.Type == WallType.Main ? true : w.Members.Any(m => m.UserId == userOrg.UserId),
                    Type = w.Type,
                    Logo = w.Logo
                })
                .ToListAsync();

            return walls;
        }

        private async Task<IList<WallDto>> GetUserFollowedWallsAsync(UserAndOrganizationDto userOrg)
        {
            var followedWalls = await _wallsDbSet
                .Include(w => w.Members)
                .Where(w => w.Type == WallType.Main || w.Type == WallType.UserCreated)
                .Join(_wallUsersDbSet, wall => wall.Id, walluser => walluser.WallId, (wall, wallUser) => new
                {
                    Id = wall.Id,
                    Name = wall.Name,
                    Description = wall.Description,
                    Type = wall.Type,
                    IsFollowing = true,
                    Logo = wall.Logo,
                    UserId = wallUser.UserId
                })
                .Where(x => x.UserId == userOrg.UserId || x.Type == WallType.Main)
                .Select(x => new WallDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type,
                    IsFollowing = true,
                    Logo = x.Logo
                })
                .Distinct()
                .ToListAsync();

            return followedWalls;
        }
    }
}
