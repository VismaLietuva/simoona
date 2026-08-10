using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public class GroupTypesService : IGroupTypesService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;
        private readonly DbSet<GroupType> _groupTypesDbSet;
        private readonly DbSet<GroupEntity> _groupsDbSet;

        public GroupTypesService(IUnitOfWork2 uow, IPermissionService permissionService)
        {
            _uow = uow;
            _permissionService = permissionService;
            _groupTypesDbSet = uow.GetDbSet<GroupType>();
            _groupsDbSet = uow.GetDbSet<GroupEntity>();
        }

        public async Task<IEnumerable<GroupTypeDto>> GetAllAsync(UserAndOrganizationDto userAndOrg)
        {
            var types = await _groupTypesDbSet
                .Include(t => t.KudosType)
                .Where(t => t.OrganizationId == userAndOrg.OrganizationId)
                .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
                .ToListAsync();

            var counts = await _groupsDbSet
                .Where(g => g.OrganizationId == userAndOrg.OrganizationId)
                .GroupBy(g => g.GroupTypeId)
                .Select(g => new { GroupTypeId = g.Key, Count = g.Count() })
                .ToListAsync();

            var showKudos = await CanEditKudosAsync(userAndOrg);

            return types
                .Select(t => MapToDto(t, counts.FirstOrDefault(c => c.GroupTypeId == t.Id)?.Count ?? 0, showKudos))
                .ToList();
        }

        public async Task<GroupTypeDto> GetAsync(int organizationId, int id)
        {
            var type = await _groupTypesDbSet
                .Include(t => t.KudosType)
                .SingleOrDefaultAsync(t => t.OrganizationId == organizationId && t.Id == id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.GroupTypeNotFound, "Group type does not exist");
            }

            var groupCount = await _groupsDbSet.CountAsync(g => g.GroupTypeId == id);

            return MapToDto(type, groupCount);
        }

        public async Task CreateAsync(CreateGroupTypeDto dto)
        {
            await ValidateNameIsUniqueAsync(dto.Name, dto.OrganizationId, null);

            if (!await CanEditKudosAsync(dto))
            {
                dto.KudosTypeId = null;
            }

            var now = DateTime.UtcNow;

            _groupTypesDbSet.Add(new GroupType
            {
                Name = dto.Name,
                OrganizationId = dto.OrganizationId,
                SortOrder = dto.SortOrder,
                IsTemporary = dto.IsTemporary,
                HasGroupTag = dto.HasGroupTag,
                CreationPolicy = dto.CreationPolicy,
                ApprovalQuestions = dto.ApprovalQuestions,
                KudosTypeId = dto.KudosTypeId,
                Created = now,
                CreatedBy = dto.UserId,
                Modified = now,
                ModifiedBy = dto.UserId
            });

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task UpdateAsync(UpdateGroupTypeDto dto)
        {
            var type = await _groupTypesDbSet
                .SingleOrDefaultAsync(t => t.OrganizationId == dto.OrganizationId && t.Id == dto.Id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.GroupTypeNotFound, "Group type does not exist");
            }

            if (!string.Equals(type.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                await ValidateNameIsUniqueAsync(dto.Name, dto.OrganizationId, dto.Id);
            }

            // Only a kudos administrator may set which kudos type these groups receive.
            // Anyone else keeps the stored value, since the form never sends it back.
            if (!await CanEditKudosAsync(dto))
            {
                dto.KudosTypeId = type.KudosTypeId;
            }

            await ClearFieldsForDisabledFlagsAsync(type, dto);

            type.Name = dto.Name;
            type.SortOrder = dto.SortOrder;
            type.IsTemporary = dto.IsTemporary;
            type.HasGroupTag = dto.HasGroupTag;
            type.CreationPolicy = dto.CreationPolicy;
            type.ApprovalQuestions = dto.ApprovalQuestions;
            type.KudosTypeId = dto.KudosTypeId;
            type.Modified = DateTime.UtcNow;
            type.ModifiedBy = dto.UserId;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userAndOrg)
        {
            var type = await _groupTypesDbSet
                .SingleOrDefaultAsync(t => t.OrganizationId == userAndOrg.OrganizationId && t.Id == id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.GroupTypeNotFound, "Group type does not exist");
            }

            var hasGroups = await _groupsDbSet.AnyAsync(g => g.GroupTypeId == id);

            if (hasGroups)
            {
                throw new ValidationException(ErrorCodes.GroupTypeHasGroups, "Group type still has groups");
            }

            _groupTypesDbSet.Remove(type);

            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        #region private

        private static GroupTypeDto MapToDto(GroupType type, int groupCount, bool showKudos = true) => new GroupTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            SortOrder = type.SortOrder,
            IsTemporary = type.IsTemporary,
            HasGroupTag = type.HasGroupTag,
            CreationPolicy = type.CreationPolicy,
            ApprovalQuestions = type.ApprovalQuestions,
            KudosTypeId = showKudos ? type.KudosTypeId : null,
            KudosTypeName = showKudos ? type.KudosType?.Name : null,
            KudosTypeValue = showKudos ? type.KudosType?.Value : null,
            GroupCount = groupCount
        };

        private Task<bool> CanEditKudosAsync(UserAndOrganizationDto userAndOrg) =>
            _permissionService.UserHasPermissionAsync(userAndOrg, AdministrationPermissions.Kudos);

        private async Task ValidateNameIsUniqueAsync(string name, int organizationId, int? excludeId)
        {
            var exists = await _groupTypesDbSet
                .AnyAsync(t => t.OrganizationId == organizationId
                            && t.Name.ToLower() == name.ToLower()
                            && (excludeId == null || t.Id != excludeId));

            if (exists)
            {
                throw new ValidationException(
                    ErrorCodes.GroupTypeNameAlreadyExists,
                    "Group type with this name already exists");
            }
        }

        /// <summary>
        /// When a flag is turned off, the field it gated is cleared on every group of the type,
        /// so no group keeps data the UI no longer shows and the server would now reject.
        /// </summary>
        private async Task ClearFieldsForDisabledFlagsAsync(GroupType type, UpdateGroupTypeDto dto)
        {
            var anyDisabled = type.IsTemporary && !dto.IsTemporary;

            if (!anyDisabled)
            {
                return;
            }

            var groups = await _groupsDbSet
                .Where(g => g.GroupTypeId == type.Id)
                .ToListAsync();

            foreach (var group in groups)
            {
                if (type.IsTemporary && !dto.IsTemporary)
                {
                    group.StartDate = null;
                    group.EndDate = null;
                }

                group.Modified = DateTime.UtcNow;
                group.ModifiedBy = dto.UserId;
            }
        }

        #endregion
    }
}
