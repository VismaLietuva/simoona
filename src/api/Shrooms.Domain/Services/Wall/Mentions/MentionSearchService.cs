using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.Domain.Services.Roles;
using ConstantsRoles = Shrooms.Contracts.Constants.Roles;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    public class MentionSearchService : IMentionSearchService
    {
        private const int PeopleLimit = 8;
        private const int GroupsLimit = 5;

        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<Group> _groupsDbSet;

        public MentionSearchService(IUnitOfWork2 uow, IRoleService roleService, ISystemClock systemClock)
        {
            _roleService = roleService;
            _systemClock = systemClock;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _groupsDbSet = uow.GetDbSet<Group>();
        }

        public async Task<MentionSuggestionsDto> SearchAsync(string search, UserAndOrganizationDto userOrg)
        {
            var term = (search ?? string.Empty).Trim();

            return new MentionSuggestionsDto
            {
                People = await SearchPeopleAsync(term, userOrg),
                Groups = await SearchGroupsAsync(term, userOrg)
            };
        }

        private async Task<MentionPersonDto[]> SearchPeopleAsync(string term, UserAndOrganizationDto userOrg)
        {
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(ConstantsRoles.NewUser);
            var hasTerm = term.Length > 0;

            var people = await _usersDbSet
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId))
                .Where(user => !hasTerm ||
                               user.FirstName.StartsWith(term) ||
                               user.LastName.StartsWith(term) ||
                               (user.FirstName + " " + user.LastName).StartsWith(term) ||
                               user.UserName.StartsWith(term) ||
                               (user.FirstName + " " + user.LastName).Contains(term))
                .OrderByDescending(user => (user.FirstName + " " + user.LastName).StartsWith(term))
                .ThenBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Take(PeopleLimit)
                .Select(user => new MentionPersonDto
                {
                    Id = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    PictureId = user.PictureId,
                    JobTitle = user.JobPosition.Title
                })
                .ToArrayAsync();

            return people;
        }

        private async Task<MentionGroupDto[]> SearchGroupsAsync(string term, UserAndOrganizationDto userOrg)
        {
            var today = _systemClock.UtcNow.Date;
            var hasTerm = term.Length > 0;

            return await _groupsDbSet
                .Where(group => group.OrganizationId == userOrg.OrganizationId &&
                                group.Status == GroupStatus.Approved &&
                                group.GroupType.HasGroupTag &&
                                (group.EndDate == null || group.EndDate >= today))
                .Where(group => !hasTerm || group.Name.StartsWith(term) || group.Name.Contains(term))
                .OrderByDescending(group => group.Name.StartsWith(term))
                .ThenBy(group => group.Name)
                .Take(GroupsLimit)
                .Select(group => new MentionGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    MemberCount = group.Members.Count(member =>
                        (member.StartDate == null || member.StartDate <= today) &&
                        (member.EndDate == null || member.EndDate >= today))
                })
                .ToArrayAsync();
        }
    }
}
