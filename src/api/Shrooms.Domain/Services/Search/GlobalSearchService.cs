using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using ConstantsRoles = Shrooms.Contracts.Constants.Roles;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Search
{
    public class GlobalSearchService : IGlobalSearchService
    {
        public const int MinSearchLength = 2;
        public const int MaxSearchLength = 50;
        public const int MaxLimit = 50;

        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<MultiwallWall> _wallsDbSet;
        private readonly DbSet<Event> _eventsDbSet;

        public GlobalSearchService(IUnitOfWork2 uow, IPermissionService permissionService, IRoleService roleService, ISystemClock systemClock)
        {
            _permissionService = permissionService;
            _roleService = roleService;
            _systemClock = systemClock;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallsDbSet = uow.GetDbSet<MultiwallWall>();
            _eventsDbSet = uow.GetDbSet<Event>();
        }

        public async Task<GlobalSearchResultsDto> SearchAsync(string search, int limit, UserAndOrganizationDto userOrg, CancellationToken cancellationToken = default)
        {
            var term = (search ?? string.Empty).Trim();
            if (term.Length > MaxSearchLength)
            {
                term = term.Substring(0, MaxSearchLength).Trim();
            }

            if (term.Length < MinSearchLength)
            {
                return new GlobalSearchResultsDto();
            }

            var take = Math.Clamp(limit, 1, MaxLimit);
            var permissions = (await _permissionService.GetUserPermissionsAsync(userOrg.UserId, userOrg.OrganizationId, cancellationToken)).ToHashSet();
            var results = new GlobalSearchResultsDto();

            // One DbContext, so the lookups run one after another rather than in parallel.
            if (permissions.Contains(BasicPermissions.EmployeeList))
            {
                results.People = await SearchPeopleAsync(term, take, userOrg, cancellationToken);
            }

            if (permissions.Contains(BasicPermissions.Post))
            {
                results.Walls = await SearchWallsAsync(term, take, userOrg, cancellationToken);
            }

            if (permissions.Contains(BasicPermissions.Event))
            {
                results.Events = await SearchEventsAsync(term, take, userOrg, cancellationToken);
            }

            return results;
        }

        // Same rule as the employee list: every word must appear in the name or the job title.
        private async Task<GlobalSearchPersonDto[]> SearchPeopleAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(ConstantsRoles.NewUser, cancellationToken);

            var query = _usersDbSet
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId));

            foreach (var word in term.Split(WebApiConstants.SearchSplitter, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Where(user =>
                    user.FirstName.Contains(word) ||
                    user.LastName.Contains(word) ||
                    user.JobPosition.Title.Contains(word));
            }

            return await query
                .OrderByDescending(user => (user.FirstName + " " + user.LastName).StartsWith(term))
                .ThenBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Take(take)
                .Select(user => new GlobalSearchPersonDto
                {
                    Id = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    JobTitle = user.JobPosition.Title,
                    PictureId = user.PictureId
                })
                .ToArrayAsync(cancellationToken);
        }

        // The walls the wall list offers: the main wall and user-created ones, never event or project walls.
        private async Task<GlobalSearchWallDto[]> SearchWallsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            return await _wallsDbSet
                .Where(wall => wall.OrganizationId == userOrg.OrganizationId &&
                               (wall.Type == WallType.Main || wall.Type == WallType.UserCreated) &&
                               wall.Name.Contains(term))
                .OrderByDescending(wall => wall.Name.StartsWith(term))
                .ThenBy(wall => wall.Name)
                .Take(take)
                .Select(wall => new GlobalSearchWallDto
                {
                    Id = wall.Id,
                    Name = wall.Name,
                    TotalMembers = wall.Members.Count
                })
                .ToArrayAsync(cancellationToken);
        }

        // Events still to come are listed ahead of finished ones.
        private async Task<GlobalSearchEventDto[]> SearchEventsAsync(string term, int take, UserAndOrganizationDto userOrg, CancellationToken cancellationToken)
        {
            var now = _systemClock.UtcNow;

            return await _eventsDbSet
                .Where(e => e.OrganizationId == userOrg.OrganizationId &&
                            (e.Name.Contains(term) || e.Place.Contains(term)))
                .OrderByDescending(e => e.EndDate >= now)
                .ThenByDescending(e => e.StartDate)
                .Take(take)
                .Select(e => new GlobalSearchEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Place = e.Place
                })
                .ToArrayAsync(cancellationToken);
        }
    }
}
