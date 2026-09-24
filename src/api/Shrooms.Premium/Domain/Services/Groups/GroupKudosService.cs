using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
        private const string GroupNamePlaceholder = "{groupname}";
        private const string RolePlaceholder = "{role}";
        private const string MonthPlaceholder = "{month}";

        private const string PlaceholderSeparator = " | ";

        private const string DefaultTemplate = "Monthly group kudos for {month}: {groupname}";

        private static readonly DateTime EarliestAwardablePeriod = new DateTime(2026, 7, 1);

        private const int MaxCatchUpMonths = 3;

        // Narrows the read-then-write window between the awarded check and the write.
        // Process-local, so a scaled-out deployment can still double-award.
        private static readonly SemaphoreSlim _awardLock = new SemaphoreSlim(1, 1);

        private static readonly Regex RepeatedSpaces = new Regex("[ ]{2,}", RegexOptions.Compiled);

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
                    .GroupBy(m => m.UserId)
                    .Select(byUser =>
                    {
                        // Several memberships of one group are either separate stints of the
                        // same role or one per role. Distinct role names are what is paid, so
                        // two jobs in a group pay twice and two stints in one job pay once.
                        var roles = byUser
                            .Select(m => m.Description)
                            .Where(d => !string.IsNullOrWhiteSpace(d))
                            .Distinct()
                            .OrderBy(d => d)
                            .ToList();

                        return new
                        {
                            UserId = byUser.Key,
                            GroupId = g.Id,
                            GroupName = g.Name,
                            Roles = roles,
                            KudosTypeId = g.GroupType.KudosTypeId.Value,
                            GroupTypeId = g.GroupTypeId,
                            g.GroupType.AwardTemplate,
                            Value = (g.GroupType.KudosType?.Value ?? 0) * Math.Max(1, roles.Count),
                            LatestEnd = byUser.Max(m => m.EndDate ?? DateTime.MaxValue),
                            LatestStart = byUser.Max(m => m.StartDate ?? DateTime.MinValue)
                        };
                    }))
                .GroupBy(a => new { a.UserId, a.GroupTypeId })
                // A type pays once a month. Someone who moved between groups of it during the
                // month is paid by the group they moved to.
                .Select(byUserAndType => byUserAndType
                    .OrderByDescending(a => a.LatestEnd)
                    .ThenByDescending(a => a.LatestStart)
                    .ThenByDescending(a => a.GroupId)
                    .First())
                .Select(a => new GroupKudosAllocationDto
                {
                    UserId = a.UserId,
                    GroupTypeId = a.GroupTypeId,
                    KudosTypeId = a.KudosTypeId,
                    AwardTemplate = a.AwardTemplate,
                    Amount = a.Value,
                    GroupNames = new List<string> { a.GroupName },
                    Roles = a.Roles
                })
                .Where(a => a.Amount > 0)
                .OrderBy(a => a.UserId)
                .ToList();
        }

        public async Task<IList<GroupMonthlyKudosResultDto>> AwardOutstandingMonthsAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month)
        {
            EnsurePeriodIsValid(year, month);
            await _awardLock.WaitAsync();

            try
            {
                var awarded = await AwardedPeriodsAsync(userAndOrg.OrganizationId);

                var outstanding = new List<DateTime>();

                var latest = new DateTime(year, month, 1);
                var oldest = latest.AddMonths(-(MaxCatchUpMonths - 1));

                if (oldest < EarliestAwardablePeriod)
                {
                    oldest = EarliestAwardablePeriod;
                }

                // Collects every unawarded month instead of stopping at the first awarded one, so a
                // month awarded on its own leaves no gap the walk could never come back for.
                for (var period = latest; period >= oldest; period = period.AddMonths(-1))
                {
                    if (!awarded.Contains(period))
                    {
                        outstanding.Add(period);
                    }
                }

                var results = new List<GroupMonthlyKudosResultDto>();

                foreach (var period in Enumerable.Reverse(outstanding))
                {
                    results.Add(await AwardPeriodAsync(userAndOrg, period.Year, period.Month));
                }

                return results;
            }
            finally
            {
                _awardLock.Release();
            }
        }

        public async Task<GroupMonthlyKudosResultDto> AwardMonthlyKudosAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month)
        {
            EnsurePeriodIsValid(year, month);
            await _awardLock.WaitAsync();

            try
            {
                if (await IsAlreadyAwardedAsync(userAndOrg.OrganizationId, year, month))
                {
                    return new GroupMonthlyKudosResultDto { Year = year, Month = month, AlreadyAwarded = true };
                }

                return await AwardPeriodAsync(userAndOrg, year, month);
            }
            finally
            {
                _awardLock.Release();
            }
        }

        private async Task<GroupMonthlyKudosResultDto> AwardPeriodAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month)
        {
            var allocations = (await GetAllocationsAsync(userAndOrg.OrganizationId, year, month)).ToList();

            var result = new GroupMonthlyKudosResultDto { Year = year, Month = month };

            if (allocations.Count == 0)
            {
                return result;
            }

            var kudosTypes = await _kudosTypesDbSet.ToListAsync();

            var now = DateTime.UtcNow;
            var period = new DateTime(year, month, 1);

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
                    Comments = RenderComment(allocation, year, month),
                    GroupKudosPeriod = period,
                    Created = now,
                    CreatedBy = userAndOrg.UserId,
                    Modified = now,
                    ModifiedBy = userAndOrg.UserId
                };

                _kudosLogsDbSet.Add(log);

                result.AwardedCount++;
                result.TotalAmount += allocation.Amount;
            }

            await _uow.SaveChangesAsync(userAndOrg.UserId);

            return result;
        }

        private static string RenderComment(GroupKudosAllocationDto allocation, int year, int month)
        {
            var template = string.IsNullOrWhiteSpace(allocation.AwardTemplate)
                ? DefaultTemplate
                : allocation.AwardTemplate;

            var rendered = template
                .Replace(GroupNamePlaceholder, string.Join(PlaceholderSeparator, allocation.GroupNames), StringComparison.OrdinalIgnoreCase)
                .Replace(RolePlaceholder, string.Join(PlaceholderSeparator, allocation.Roles), StringComparison.OrdinalIgnoreCase)
                .Replace(MonthPlaceholder, Period(year, month), StringComparison.OrdinalIgnoreCase);

            // A placeholder with nothing to render leaves the spaces that framed it.
            rendered = RepeatedSpaces.Replace(rendered, " ").Trim();

            // Comments is required, and a template of nothing but placeholders can render empty.
            return string.IsNullOrWhiteSpace(rendered)
                ? $"Monthly group kudos for {Period(year, month)}"
                : rendered;
        }

        private async Task<bool> IsAlreadyAwardedAsync(int organizationId, int year, int month)
        {
            var period = new DateTime(year, month, 1);

            return await _kudosLogsDbSet
                .AsNoTracking()
                .AnyAsync(log => log.OrganizationId == organizationId
                             && log.GroupKudosPeriod == period);
        }

        private async Task<HashSet<DateTime>> AwardedPeriodsAsync(int organizationId)
        {
            var periods = await _kudosLogsDbSet
                .AsNoTracking()
                .Where(log => log.OrganizationId == organizationId
                           && log.GroupKudosPeriod >= EarliestAwardablePeriod)
                .Select(log => log.GroupKudosPeriod.Value)
                .Distinct()
                .ToListAsync();

            return new HashSet<DateTime>(periods);
        }

        private static string Period(int year, int month) => $"{year}-{month:00}";

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
