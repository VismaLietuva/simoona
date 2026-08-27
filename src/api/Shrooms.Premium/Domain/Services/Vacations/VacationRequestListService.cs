using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using X.PagedList;
using X.PagedList.EF;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationRequestListService : IVacationRequestListService
    {
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<VacationRequestEvent> _eventDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        public VacationRequestListService(IUnitOfWork2 uow)
        {
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _eventDbSet = uow.GetDbSet<VacationRequestEvent>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<IPagedList<VacationRequestDto>> GetMyRequestsAsync(VacationRequestArgsDto args)
        {
            var query = BaseQuery(args).Where(request => request.EmployeeId == args.UserId);

            // Entitlement, so an edit is measured against a balance that excludes
            // the request being edited.
            return await PageAsync(query, args, Enrichment.Entitlement);
        }

        public async Task<IPagedList<VacationRequestDto>> GetTeamRequestsAsync(VacationRequestArgsDto args)
        {
            var query = BaseQuery(args).Where(request => request.Employee.ManagerId == args.UserId);

            return await PageAsync(query, args, Enrichment.Full);
        }

        public async Task<IPagedList<VacationRequestDto>> GetAllRequestsAsync(VacationRequestArgsDto args)
        {
            // No overlap or edit hints: the register is a record, not a queue.
            return await PageAsync(BaseQuery(args), args, Enrichment.Entitlement);
        }

        public async Task<int> GetPendingTeamCountAsync(UserAndOrganizationDto userOrg)
        {
            return await _requestDbSet
                .AsNoTracking()
                .CountAsync(request => request.OrganizationId == userOrg.OrganizationId
                                       && request.Employee.ManagerId == userOrg.UserId
                                       && request.Status == VacationRequestStatus.Pending);
        }

        public async Task<bool> HasDirectReportsAsync(UserAndOrganizationDto userOrg)
        {
            return await _userDbSet
                .AsNoTracking()
                .AnyAsync(user => user.OrganizationId == userOrg.OrganizationId
                                  && user.ManagerId == userOrg.UserId);
        }

        [Flags]
        private enum Enrichment
        {
            None = 0,

            Entitlement = 1,

            Overlaps = 2,

            LastEdit = 4,

            Full = Entitlement | Overlaps | LastEdit
        }

        private IQueryable<VacationRequest> BaseQuery(VacationRequestArgsDto args)
        {
            var query = _requestDbSet
                .AsNoTracking()
                // The employee navigation is required, so a soft-deleted user
                // takes their whole history out of the join with them.
                .IgnoreQueryFilters()
                .Include(request => request.Employee)
                .Include(request => request.ReviewedBy)
                .Where(request => request.OrganizationId == args.OrganizationId);

            var status = VacationWireFormat.ParseStatus(args.Status);
            if (status != null)
            {
                query = query.Where(request => request.Status == status.Value);
            }

            var type = VacationWireFormat.ParseType(args.Type);
            if (type != null)
            {
                query = query.Where(request => request.Type == type.Value);
            }

            // Overlap, not containment: filtering on DateFrom alone hid every
            // request that started before the window and ran into it.
            if (args.From.HasValue)
            {
                var from = args.From.Value.Date;
                query = query.Where(request => request.DateTo >= from);
            }

            if (args.To.HasValue)
            {
                var to = args.To.Value.Date;
                query = query.Where(request => request.DateFrom <= to);
            }

            if (!string.IsNullOrWhiteSpace(args.Search))
            {
                var term = args.Search.Trim();
                query = query.Where(request =>
                    request.Employee.FirstName.Contains(term)
                    || request.Employee.LastName.Contains(term)
                    || request.Note.Contains(term));
            }

            return query;
        }

        private static IQueryable<VacationRequest> ApplySort(IQueryable<VacationRequest> query, VacationRequestArgsDto args)
        {
            var descending = !string.Equals(args.Dir, "asc", StringComparison.OrdinalIgnoreCase);

            // Tie-break outside the direction, or a descending sort reverses an
            // employee's own requests among themselves.
            return (args.Sort ?? string.Empty).Trim() switch
            {
                "employee" => descending
                    ? query.OrderByDescending(r => r.Employee.FirstName).ThenByDescending(r => r.Employee.LastName).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.Employee.FirstName).ThenBy(r => r.Employee.LastName).ThenBy(r => r.Id),
                "dateFrom" => descending
                    ? query.OrderByDescending(r => r.DateFrom).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.DateFrom).ThenBy(r => r.Id),
                "type" => descending
                    ? query.OrderByDescending(r => r.Type).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.Type).ThenBy(r => r.Id),
                "workingDays" => descending
                    ? query.OrderByDescending(r => r.WorkingDays).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.WorkingDays).ThenBy(r => r.Id),
                "status" => descending
                    ? query.OrderByDescending(r => r.Status).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.Status).ThenBy(r => r.Id),
                _ => descending
                    ? query.OrderByDescending(r => r.Created).ThenBy(r => r.Id)
                    : query.OrderBy(r => r.Created).ThenBy(r => r.Id)
            };
        }

        private async Task<IPagedList<VacationRequestDto>> PageAsync(
            IQueryable<VacationRequest> query,
            VacationRequestArgsDto args,
            Enrichment enrichment)
        {
            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == args.OrganizationId);

            var today = VacationCalculator.TodayIn(organization?.TimeZone);

            var page = await ApplySort(query, args).ToPagedListAsync(args.Page, args.PageSize);
            var rows = page.Select(request => VacationMapper.ToRequest(request, today)).ToList();

            if (enrichment != Enrichment.None && rows.Count > 0)
            {
                await EnrichAsync(page.ToList(), rows, args.OrganizationId, enrichment);
            }

            return new StaticPagedList<VacationRequestDto>(rows, page.PageNumber, page.PageSize, page.TotalItemCount);
        }

        /// <summary>
        /// Batch-loaded for the whole page: a per-row version issued three queries
        /// each, which on a fifty-row register is a hundred and fifty round trips.
        /// </summary>
        private async Task EnrichAsync(
            IReadOnlyList<VacationRequest> entities,
            IReadOnlyList<VacationRequestDto> rows,
            int organizationId,
            Enrichment enrichment)
        {
            var employeeIds = entities.Select(request => request.EmployeeId).Distinct().ToList();

            var entitlements = await _userDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(user => employeeIds.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    Unused = user.VacationUnusedTime,
                    AsOf = user.VacationLastTimeUpdated
                })
                .ToListAsync();

            var entitlementById = entitlements.ToDictionary(e => e.Id);

            // The committed-days sum cannot be computed from the page alone.
            var activeAnnual = await _requestDbSet
                .AsNoTracking()
                .Where(request => request.OrganizationId == organizationId
                                  && employeeIds.Contains(request.EmployeeId)
                                  && request.Type == VacationRequestType.Annual
                                  && (request.Status == VacationRequestStatus.Pending
                                      || request.Status == VacationRequestStatus.Approved))
                .ToListAsync();

            var annualByEmployee = activeAnnual
                .GroupBy(request => request.EmployeeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var overlapsByRequest = enrichment.HasFlag(Enrichment.Overlaps)
                ? await LoadOverlapsAsync(entities, organizationId)
                : new Dictionary<int, List<VacationOverlapDto>>();

            var lastEditByRequest = enrichment.HasFlag(Enrichment.LastEdit)
                ? await LoadLastEditsAsync(entities, organizationId)
                : new Dictionary<int, VacationLastEditDto>();

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var row = rows[i];

                if (!entitlementById.TryGetValue(entity.EmployeeId, out var entitlement))
                {
                    continue;
                }

                var accrued = entitlement.Unused ?? 0;
                row.Entitlement = accrued;

                var own = annualByEmployee.TryGetValue(entity.EmployeeId, out var list)
                    ? list
                    : new List<VacationRequest>();

                var deducts = VacationCalculator.DeductsBalance(entity.Type);

                // A charging request is excluded from its own total, or it always
                // looks over budget. A parental or unpaid one is not in that total
                // to begin with, so nothing is excluded and nothing subtracted.
                var remaining = accrued - VacationCalculator.CommittedAnnualDays(
                    own,
                    entitlement.AsOf?.Date,
                    deducts ? entity.Id : null);

                row.RemainingDays = remaining;

                if (deducts && entity.WorkingDays > remaining)
                {
                    row.BalanceShortfall = new VacationShortfallDto
                    {
                        Requested = entity.WorkingDays,
                        Remaining = remaining
                    };
                }

                if (overlapsByRequest.TryGetValue(entity.Id, out var overlaps))
                {
                    row.Overlaps = overlaps;
                }

                if (lastEditByRequest.TryGetValue(entity.Id, out var lastEdit))
                {
                    row.LastEdit = lastEdit;
                }
            }
        }

        private async Task<Dictionary<int, List<VacationOverlapDto>>> LoadOverlapsAsync(
            IReadOnlyList<VacationRequest> entities,
            int organizationId)
        {
            var active = entities.Where(request => VacationCalculator.IsActive(request.Status)).ToList();
            if (active.Count == 0)
            {
                return new Dictionary<int, List<VacationOverlapDto>>();
            }

            var windowFrom = active.Min(request => request.DateFrom);
            var windowTo = active.Max(request => request.DateTo);

            var candidates = await _requestDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(request => request.Employee)
                .Where(request => request.OrganizationId == organizationId
                                  && (request.Status == VacationRequestStatus.Pending
                                      || request.Status == VacationRequestStatus.Approved)
                                  && request.DateTo >= windowFrom
                                  && request.DateFrom <= windowTo)
                .ToListAsync();

            var projectsByEmployee = await LoadProjectMembershipAsync(
                active.Select(request => request.EmployeeId)
                    .Concat(candidates.Select(candidate => candidate.EmployeeId)),
                organizationId);

            var result = new Dictionary<int, List<VacationOverlapDto>>();

            foreach (var request in active)
            {
                var projects = Projects(projectsByEmployee, request.EmployeeId);

                var overlaps = candidates
                    .Where(other => other.Id != request.Id
                                    // Own other leave is not a staffing clash.
                                    && other.EmployeeId != request.EmployeeId
                                    && projects.Overlaps(Projects(projectsByEmployee, other.EmployeeId))
                                    && VacationCalculator.RangesOverlap(request.DateFrom, request.DateTo, other.DateFrom, other.DateTo))
                    .Select(other => new VacationOverlapDto
                    {
                        Employee = VacationMapper.ToPerson(other.Employee),
                        DateFrom = VacationWireFormat.ToDay(other.DateFrom),
                        DateTo = VacationWireFormat.ToDay(other.DateTo)
                    })
                    .ToList();

                if (overlaps.Count > 0)
                {
                    result[request.Id] = overlaps;
                }
            }

            return result;
        }

        private static HashSet<int> Projects(
            IReadOnlyDictionary<string, HashSet<int>> membership,
            string employeeId)
        {
            return membership.TryGetValue(employeeId, out var projects) ? projects : Empty;
        }

        private static readonly HashSet<int> Empty = new HashSet<int>();

        private async Task<Dictionary<string, HashSet<int>>> LoadProjectMembershipAsync(
            IEnumerable<string> employeeIds,
            int organizationId)
        {
            var ids = employeeIds.Distinct().ToList();

            var rows = await _userDbSet
                .AsNoTracking()
                .Where(user => user.OrganizationId == organizationId && ids.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    ProjectIds = user.Projects.Select(project => project.Id).ToList()
                })
                .ToListAsync();

            return rows.ToDictionary(row => row.Id, row => row.ProjectIds.ToHashSet());
        }

        private async Task<Dictionary<int, VacationLastEditDto>> LoadLastEditsAsync(
            IReadOnlyList<VacationRequest> entities,
            int organizationId)
        {
            var ids = entities.Select(request => request.Id).ToList();

            var edits = await _eventDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(entity => entity.Actor)
                .Where(entity => entity.OrganizationId == organizationId
                                 && entity.Kind == VacationEventKind.Edited
                                 && ids.Contains(entity.VacationRequestId))
                .ToListAsync();

            return edits
                .GroupBy(entity => entity.VacationRequestId)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var latest = group.OrderByDescending(entity => entity.OccurredAt).ThenByDescending(entity => entity.Id).First();
                        return new VacationLastEditDto
                        {
                            At = DateTime.SpecifyKind(latest.OccurredAt, DateTimeKind.Utc),
                            Actor = VacationMapper.ToPerson(latest.Actor),
                            Changes = VacationMapper.DeserializeChanges(latest.ChangesJson)
                        };
                    });
        }
    }
}
