using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public class GroupsService : IGroupsService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;
        private readonly DbSet<GroupEntity> _groupsDbSet;
        private readonly DbSet<GroupType> _groupTypesDbSet;
        private readonly DbSet<ApplicationUser> _usersDbSet;

        public GroupsService(IUnitOfWork2 uow, IPermissionService permissionService)
        {
            _uow = uow;
            _permissionService = permissionService;
            _groupsDbSet = uow.GetDbSet<GroupEntity>();
            _groupTypesDbSet = uow.GetDbSet<GroupType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<IEnumerable<GroupDto>> GetAllAsync(UserAndOrganizationDto userAndOrg)
        {
            var groups = await _groupsDbSet
                .Include(g => g.GroupType).ThenInclude(t => t.KudosType)
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.References)
                .Where(g => g.OrganizationId == userAndOrg.OrganizationId)
                .OrderBy(g => g.Name)
                .ToListAsync();

            var utcNow = DateTime.UtcNow;
            var isAdmin = await _permissionService.UserHasPermissionAsync(userAndOrg, AdministrationPermissions.Groups);

            // A group awaiting approval is only visible to its members and to administrators.
            return groups
                .Where(g => !g.IsPending || isAdmin || IsActiveMember(g, userAndOrg.UserId))
                .Select(g => MapToDto(g, utcNow, userAndOrg.UserId))
                .ToList();
        }

        public async Task<GroupDto> GetAsync(UserAndOrganizationDto userAndOrg, int id)
        {
            var group = await _groupsDbSet
                .Include(g => g.GroupType).ThenInclude(t => t.KudosType)
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.References)
                .SingleOrDefaultAsync(g => g.OrganizationId == userAndOrg.OrganizationId && g.Id == id);

            if (group == null)
            {
                throw new ValidationException(ErrorCodes.GroupNotFound, "Group does not exist");
            }

            return MapToDto(group, DateTime.UtcNow, userAndOrg.UserId);
        }

        public async Task CreateAsync(GroupPostDto dto)
        {
            var type = await GetTypeAsync(dto.GroupTypeId, dto.OrganizationId);
            var isAdmin = await _permissionService.UserHasPermissionAsync(dto, AdministrationPermissions.Groups);

            EnsureCanCreate(type, isAdmin);
            ValidateApprovalAnswers(type, dto, isAdmin);

            var now = DateTime.UtcNow;

            var group = new GroupEntity
            {
                OrganizationId = dto.OrganizationId,
                // An administrator's own group needs no approval - they are the approver.
                Status = type.CreationPolicy == GroupCreationPolicy.RequiresApproval && !isAdmin
                    ? GroupStatus.Pending
                    : GroupStatus.Approved,
                Created = now,
                CreatedBy = dto.UserId,
                Modified = now,
                ModifiedBy = dto.UserId
            };

            await ValidateAsync(dto, type, null);

            ApplyPost(group, dto);
            group.Members = await ReconcileMembersAsync(null, dto.Members, isAdmin);
            group.References = ResolveReferences(dto.References);

            // The creator joins their own group, so they can see and edit it while it waits.
            if (!isAdmin && group.Members.All(m => m.UserId != dto.UserId))
            {
                group.Members.Add(new GroupMember { UserId = dto.UserId });
            }

            _groupsDbSet.Add(group);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task ApproveAsync(int id, UserAndOrganizationDto userAndOrg)
        {
            var group = await _groupsDbSet
                .SingleOrDefaultAsync(g => g.OrganizationId == userAndOrg.OrganizationId && g.Id == id);

            if (group == null)
            {
                throw new ValidationException(ErrorCodes.GroupNotFound, "Group does not exist");
            }

            group.Status = GroupStatus.Approved;
            group.Modified = DateTime.UtcNow;
            group.ModifiedBy = userAndOrg.UserId;

            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        public async Task UpdateAsync(GroupPostDto dto)
        {
            var group = await _groupsDbSet
                .Include(g => g.Members)
                .Include(g => g.References)
                .SingleOrDefaultAsync(g => g.OrganizationId == dto.OrganizationId && g.Id == dto.Id);

            if (group == null)
            {
                throw new ValidationException(ErrorCodes.GroupNotFound, "Group does not exist");
            }

            var isAdmin = await _permissionService.UserHasPermissionAsync(dto, AdministrationPermissions.Groups);

            EnsureCanEdit(group, dto, isAdmin);

            // Name and type are settled once a group is approved - changing either would
            // make it a different group. A pending request can still be reshaped by its
            // creator, and an administrator can change both at any point.
            if (!isAdmin && !group.IsPending)
            {
                dto.Name = group.Name;
                dto.GroupTypeId = group.GroupTypeId;
            }

            var type = await GetTypeAsync(dto.GroupTypeId, dto.OrganizationId);

            await ValidateAsync(dto, type, dto.Id);

            // A caller who is not a member never received the group's non-public references,
            // so they cannot be in the payload. Carry them over instead of deleting them.
            var hiddenFromCaller = IsActiveMember(group, dto.UserId)
                ? new List<GroupReference>()
                : (group.References ?? new List<GroupReference>()).Where(r => !r.IsPubliclyVisible).ToList();

            ApplyPost(group, dto);
            group.Members = await ReconcileMembersAsync(group.Members, dto.Members, isAdmin);
            group.References = ResolveReferences(dto.References).Concat(hiddenFromCaller).ToList();
            group.Modified = DateTime.UtcNow;
            group.ModifiedBy = dto.UserId;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userAndOrg)
        {
            var group = await _groupsDbSet
                .SingleOrDefaultAsync(g => g.OrganizationId == userAndOrg.OrganizationId && g.Id == id);

            if (group == null)
            {
                throw new ValidationException(ErrorCodes.GroupNotFound, "Group does not exist");
            }

            await EnsureCanDeleteAsync(group, userAndOrg);

            _groupsDbSet.Remove(group);

            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        #region private

        private async Task<GroupType> GetTypeAsync(int groupTypeId, int organizationId)
        {
            var type = await _groupTypesDbSet
                .SingleOrDefaultAsync(t => t.OrganizationId == organizationId && t.Id == groupTypeId);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.GroupTypeNotFound, "Group type does not exist");
            }

            return type;
        }

        /// <summary>
        /// Rejects any field the group's type does not enable, then checks the rules
        /// that only apply once a field is enabled.
        /// </summary>
        private async Task ValidateAsync(GroupPostDto dto, GroupType type, int? excludeId)
        {
            var members = dto.Members ?? new List<GroupMemberPostDto>();

            RejectDisabledField(!type.IsTemporary && (dto.StartDate.HasValue || dto.EndDate.HasValue), "Dates");

            // Both dates are optional even on temporary types - a task force may not know
            // its end date yet. Only their order is enforced.
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate.Value)
            {
                throw new ValidationException(
                    ErrorCodes.GroupEndDateBeforeStartDate,
                    "End date cannot be before start date");
            }

            foreach (var member in members.Where(m => m.StartDate.HasValue && m.EndDate.HasValue))
            {
                if (member.EndDate.Value < member.StartDate.Value)
                {
                    throw new ValidationException(
                        ErrorCodes.GroupEndDateBeforeStartDate,
                        "Member end date cannot be before their start date");
                }
            }

            var nameTaken = await _groupsDbSet.AnyAsync(g => g.OrganizationId == dto.OrganizationId
                                                          && g.Name.ToLower() == dto.Name.ToLower()
                                                          && (excludeId == null || g.Id != excludeId));

            // Name doubles as the handle a group is tagged by in posts, so it must stay unique.
            if (nameTaken)
            {
                throw new ValidationException(ErrorCodes.GroupNameAlreadyExists, "Group name already exists");
            }
        }

        /// <summary>
        /// A group is editable by a groups administrator, or by anyone currently in it.
        /// </summary>
        private static void EnsureCanEdit(GroupEntity group, UserAndOrganizationDto userAndOrg, bool isAdmin)
        {
            if (isAdmin)
            {
                return;
            }

            if (!IsActiveMember(group, userAndOrg.UserId))
            {
                throw new ValidationException(
                    ErrorCodes.GroupEditNotAllowed,
                    "Only a groups administrator or a member of the group can edit it");
            }
        }

        /// <summary>
        /// Administrators can always delete. Whoever raised a request can withdraw it
        /// while it is still pending, but not once it has been approved.
        /// </summary>
        private async Task EnsureCanDeleteAsync(GroupEntity group, UserAndOrganizationDto userAndOrg)
        {
            if (await _permissionService.UserHasPermissionAsync(userAndOrg, AdministrationPermissions.Groups))
            {
                return;
            }

            if (group.IsPending && group.CreatedBy == userAndOrg.UserId)
            {
                return;
            }

            throw new ValidationException(
                ErrorCodes.GroupDeleteNotAllowed,
                "Only a groups administrator, or the creator of a group still awaiting approval, can delete it");
        }

        private static bool IsActiveMember(GroupEntity group, string userId)
        {
            var today = DateTime.UtcNow.Date;

            return group.Members != null
                && group.Members.Any(m => m.UserId == userId && m.IsActiveDuring(today, today));
        }

        private static void EnsureCanCreate(GroupType type, bool isAdmin)
        {
            if (isAdmin || type.CreationPolicy != GroupCreationPolicy.AdminOnly)
            {
                return;
            }

            throw new ValidationException(
                ErrorCodes.GroupCreationNotAllowed,
                "Only a groups administrator can create groups of this type");
        }

        /// <summary>
        /// A type that needs approval asks its questions up front, so the answers are
        /// required from anyone whose group will actually go through approval.
        /// </summary>
        private static void ValidateApprovalAnswers(GroupType type, GroupPostDto dto, bool isAdmin)
        {
            if (isAdmin
                || type.CreationPolicy != GroupCreationPolicy.RequiresApproval
                || string.IsNullOrWhiteSpace(type.ApprovalQuestions))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.ApprovalAnswers))
            {
                throw new ValidationException(
                    ErrorCodes.GroupApprovalAnswersRequired,
                    "This group type requires the approval questions to be answered");
            }
        }

        private static void RejectDisabledField(bool isPopulated, string fieldName)
        {
            if (isPopulated)
            {
                throw new ValidationException(
                    ErrorCodes.GroupFieldNotAllowedByType,
                    $"{fieldName} is not enabled by this group type");
            }
        }

        private static void ApplyPost(GroupEntity group, GroupPostDto dto)
        {
            group.Name = dto.Name;
            group.Description = dto.Description;
            group.PictureId = dto.PictureId;
            group.GroupTypeId = dto.GroupTypeId;
            group.ApprovalAnswers = dto.ApprovalAnswers;
            group.StartDate = dto.StartDate;
            group.EndDate = dto.EndDate;
        }

        /// <summary>
        /// Matches submitted rows to stored ones by MembershipId so existing memberships
        /// keep their identity instead of being deleted and recreated on every save.
        /// For anyone who is not an administrator, a start date that is already set is
        /// left alone, and a membership that has begun cannot be dropped - it is ended.
        /// </summary>
        private async Task<ICollection<GroupMember>> ReconcileMembersAsync(
            ICollection<GroupMember> stored,
            ICollection<GroupMemberPostDto> submitted,
            bool isAdmin)
        {
            var current = stored ?? new List<GroupMember>();
            var payload = submitted ?? new List<GroupMemberPostDto>();

            var userIds = payload.Select(m => m.Id).ToList();
            var knownUserIds = await _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var kept = payload.Where(m => knownUserIds.Contains(m.Id)).ToList();
            var keptRowIds = new HashSet<int>(kept.Where(m => m.MembershipId.HasValue).Select(m => m.MembershipId.Value));

            if (!isAdmin)
            {
                var today = DateTime.UtcNow.Date;

                if (current.Any(m => m.HasStarted(today) && !keptRowIds.Contains(m.Id)))
                {
                    throw new ValidationException(
                        ErrorCodes.GroupMemberCannotBeRemoved,
                        "A membership that has already started cannot be removed - set its end date instead");
                }
            }

            var result = new List<GroupMember>();

            foreach (var member in kept)
            {
                var row = member.MembershipId.HasValue
                    ? current.FirstOrDefault(c => c.Id == member.MembershipId.Value)
                    : null;

                if (row == null)
                {
                    result.Add(new GroupMember
                    {
                        UserId = member.Id,
                        Description = member.Description,
                        StartDate = member.StartDate,
                        EndDate = member.EndDate
                    });

                    continue;
                }

                row.Description = member.Description;
                row.EndDate = member.EndDate;

                // Once a start date exists it is an administrator's to change.
                if (isAdmin || !row.StartDate.HasValue)
                {
                    row.StartDate = member.StartDate;
                }

                result.Add(row);
            }

            return result;
        }

        private static ICollection<GroupReference> ResolveReferences(ICollection<GroupReferenceDto> references)
        {
            if (references == null)
            {
                return new List<GroupReference>();
            }

            return references
                .Where(r => !string.IsNullOrWhiteSpace(r.Url))
                .Select(r => new GroupReference
                {
                    Url = r.Url.Trim(),
                    Name = string.IsNullOrWhiteSpace(r.Name) ? r.Url.Trim() : r.Name.Trim(),
                    IsPubliclyVisible = r.IsPubliclyVisible
                })
                .ToList();
        }

        private async Task<ICollection<ApplicationUser>> ResolveUsersAsync(
            ICollection<ApplicationUserMinimalDto> users)
        {
            if (users == null || !users.Any())
            {
                return new List<ApplicationUser>();
            }

            var ids = users.Select(u => u.Id).ToList();

            return await _usersDbSet.Where(u => ids.Contains(u.Id)).ToListAsync();
        }

        private static GroupDto MapToDto(GroupEntity group, DateTime utcNow, string currentUserId) => new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            PictureId = group.PictureId,
            GroupTypeId = group.GroupTypeId,
            GroupTypeName = group.GroupType?.Name,
            GroupType = group.GroupType == null ? null : new GroupTypeDto
            {
                Id = group.GroupType.Id,
                Name = group.GroupType.Name,
                SortOrder = group.GroupType.SortOrder,
                IsTemporary = group.GroupType.IsTemporary,
                HasGroupTag = group.GroupType.HasGroupTag,
                CreationPolicy = group.GroupType.CreationPolicy,
                ApprovalQuestions = group.GroupType.ApprovalQuestions,
                KudosTypeId = group.GroupType.KudosTypeId,
                KudosTypeName = group.GroupType.KudosType?.Name,
                KudosTypeValue = group.GroupType.KudosType?.Value
            },
            StartDate = group.StartDate,
            EndDate = group.EndDate,
            Status = group.Status,
            IsPending = group.IsPending,
            CreatedBy = group.CreatedBy,
            ApprovalAnswers = group.ApprovalAnswers,
            IsExpired = group.IsExpired(utcNow),
            Members = group.Members?.Select(MapMember).ToList() ?? new List<GroupMemberDto>(),
            References = MapReferences(group, currentUserId)
        };

        /// <summary>
        /// Non-public references are only exposed to people currently in the group.
        /// </summary>
        private static ICollection<GroupReferenceDto> MapReferences(GroupEntity group, string currentUserId)
        {
            if (group.References == null)
            {
                return new List<GroupReferenceDto>();
            }

            var isMember = IsActiveMember(group, currentUserId);

            return group.References
                .Where(r => r.IsPubliclyVisible || isMember)
                .Select(r => new GroupReferenceDto
                {
                    Id = r.Id,
                    Url = r.Url,
                    Name = r.Name,
                    IsPubliclyVisible = r.IsPubliclyVisible
                })
                .ToList();
        }

        private static GroupMemberDto MapMember(GroupMember member) => new GroupMemberDto
        {
            MembershipId = member.Id,
            Id = member.UserId,
            FirstName = member.User?.FirstName,
            LastName = member.User?.LastName,
            PictureId = member.User?.PictureId,
            Description = member.Description,
            StartDate = member.StartDate,
            EndDate = member.EndDate
        };

        private static ApplicationUserMinimalDto MapUser(ApplicationUser user) => user == null
            ? null
            : new ApplicationUserMinimalDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PictureId = user.PictureId
            };

        #endregion
    }
}
