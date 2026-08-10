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
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public class GroupKudosService : IGroupKudosService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<GroupEntity> _groupsDbSet;
        private readonly DbSet<KudosLog> _kudosLogsDbSet;
        private readonly DbSet<KudosType> _kudosTypesDbSet;

        public GroupKudosService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _groupsDbSet = uow.GetDbSet<GroupEntity>();
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
        }

        /// <summary>
        /// Each kudos-receiving group a person belongs to contributes its kudos type's value.
        /// Those are summed per person and per kudos type, so someone in three food teams
        /// gets three times that type's value.
        /// Temporary groups are excluded: they pay out once at the end of their term,
        /// not every month. So are groups still awaiting approval - a proposal has not
        /// earned anything yet.
        /// </summary>
        public async Task<IEnumerable<GroupKudosAllocationDto>> GetAllocationsAsync(int organizationId, int year, int month)
        {
            EnsurePeriodIsValid(year, month);

            var groups = await _groupsDbSet
                .Include(g => g.GroupType).ThenInclude(t => t.KudosType)
                .Include(g => g.Members)
                .Where(g => g.OrganizationId == organizationId
                         && g.Status == GroupStatus.Approved
                         && g.GroupType.KudosTypeId != null
                         && !g.GroupType.IsTemporary)
                .ToListAsync();

            var periodStart = new DateTime(year, month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            return groups
                .SelectMany(g => (g.Members ?? new List<GroupMember>())
                    .Where(m => m.IsActiveDuring(periodStart, periodEnd))
                    // One person can hold several memberships of a group; only pay once per group.
                    .Select(m => m.UserId)
                    .Distinct()
                    .Select(userId => new
                    {
                        UserId = userId,
                        GroupName = g.Name,
                        KudosTypeId = g.GroupType.KudosTypeId.Value,
                        Value = g.GroupType.KudosType?.Value ?? 0
                    }))
                .GroupBy(a => new { a.UserId, a.KudosTypeId })
                .Select(byUserAndType => new GroupKudosAllocationDto
                {
                    UserId = byUserAndType.Key.UserId,
                    KudosTypeId = byUserAndType.Key.KudosTypeId,
                    Amount = byUserAndType.Sum(a => a.Value),
                    GroupNames = byUserAndType.Select(a => a.GroupName).OrderBy(n => n).ToList()
                })
                .Where(a => a.Amount > 0)
                .OrderBy(a => a.UserId)
                .ToList();
        }

        /// <summary>
        /// Writes one pending KudosLog entry per allocated member, so the monthly run goes
        /// through the same approval a kudos admin gives anything else. Profile balances
        /// are recomputed on approval, not here - a pending log counts towards nothing.
        /// Not idempotent - the external job owns scheduling, so calling it twice for a
        /// month awards twice.
        /// </summary>
        public async Task<GroupMonthlyKudosResultDto> AwardMonthlyKudosAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month)
        {
            var allocations = (await GetAllocationsAsync(userAndOrg.OrganizationId, year, month)).ToList();

            var kudosTypes = await _kudosTypesDbSet.ToListAsync();

            var result = new GroupMonthlyKudosResultDto { Year = year, Month = month };
            var now = DateTime.UtcNow;

            foreach (var allocation in allocations)
            {
                var kudosType = kudosTypes.FirstOrDefault(k => k.Id == allocation.KudosTypeId);

                var log = new KudosLog
                {
                    OrganizationId = userAndOrg.OrganizationId,
                    EmployeeId = allocation.UserId,
                    KudosTypeName = kudosType?.Name,
                    KudosTypeValue = kudosType?.Value ?? 1,
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    Status = KudosStatus.Pending,
                    Points = allocation.Amount,
                    MultiplyBy = 1,
                    Comments = $"Monthly group kudos for {string.Join(", ", allocation.GroupNames)}",
                    Created = now,
                    CreatedBy = userAndOrg.UserId,
                    Modified = now,
                    ModifiedBy = userAndOrg.UserId
                };

                _kudosLogsDbSet.Add(log);

                result.AwardedCount++;
                result.TotalAmount += allocation.Amount;
            }

            if (result.AwardedCount > 0)
            {
                await _uow.SaveChangesAsync(userAndOrg.UserId);
            }

            return result;
        }

        /// <summary>
        /// Year and month arrive straight off the query string, and the period is built
        /// from them - an out-of-range value would otherwise surface as a 500.
        /// </summary>
        private static void EnsurePeriodIsValid(int year, int month)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12)
            {
                throw new ValidationException(
                    ErrorCodes.GroupInvalidKudosPeriod,
                    "Year must be between 1 and 9999, and month between 1 and 12");
            }
        }
    }
}
