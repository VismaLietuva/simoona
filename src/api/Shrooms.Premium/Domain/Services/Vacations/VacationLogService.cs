using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using X.PagedList;
using X.PagedList.EF;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationLogService : IVacationLogService
    {
        private readonly DbSet<VacationRequestEvent> _eventDbSet;
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        public VacationLogService(IUnitOfWork2 uow)
        {
            _eventDbSet = uow.GetDbSet<VacationRequestEvent>();
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<IPagedList<VacationEventDto>> GetLogAsync(VacationLogArgsDto args)
        {
            var query = _eventDbSet
                .AsNoTracking()
                // The log is append-only, and a row must not disappear from it
                // because the person it names has since been soft-deleted.
                .IgnoreQueryFilters()
                .Include(entity => entity.Actor)
                .Include(entity => entity.Employee)
                .Where(entity => entity.OrganizationId == args.OrganizationId);

            var kind = VacationWireFormat.ParseKind(args.Kind);
            if (kind != null)
            {
                query = query.Where(entity => entity.Kind == kind.Value);
            }

            var type = VacationWireFormat.ParseType(args.Type);
            if (type != null)
            {
                query = query.Where(entity => entity.Type == type.Value);
            }

            // These bounds are on the event's own day, not the leave period it
            // describes. OccurredAt is UTC, so they are shifted into UTC first, or
            // a late-evening action is filed under the following day. The offset is
            // taken at each bound rather than today, or a query across a daylight
            // saving change is an hour out.
            var timeZone = await _organizationDbSet.TimeZoneAsync(args.OrganizationId);

            if (args.From.HasValue)
            {
                var from = VacationCalculator.ToUtcFrom(args.From.Value.Date, timeZone);
                query = query.Where(entity => entity.OccurredAt >= from);
            }

            if (args.To.HasValue)
            {
                var to = VacationCalculator.ToUtcFrom(args.To.Value.Date.AddDays(1), timeZone);
                query = query.Where(entity => entity.OccurredAt < to);
            }

            if (!string.IsNullOrWhiteSpace(args.EmployeeId))
            {
                query = query.Where(entity => entity.EmployeeId == args.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(args.Search))
            {
                var term = args.Search.Trim();
                query = query.Where(entity =>
                    ((entity.Employee.FirstName ?? "") + " " + (entity.Employee.LastName ?? "")).Contains(term)
                    || ((entity.Employee.LastName ?? "") + " " + (entity.Employee.FirstName ?? "")).Contains(term)
                    || ((entity.Actor.FirstName ?? "") + " " + (entity.Actor.LastName ?? "")).Contains(term)
                    || ((entity.Actor.LastName ?? "") + " " + (entity.Actor.FirstName ?? "")).Contains(term)
                    || entity.Comment.Contains(term));
            }

            var page = await ApplySort(query, args).ToPagedListAsync(args.Page, args.PageSize);
            var rows = page.Select(VacationMapper.ToEvent).ToList();

            await AttachLiveRequestsAsync(rows, args.OrganizationId);

            return new StaticPagedList<VacationEventDto>(
                rows,
                page.PageNumber,
                page.PageSize,
                page.TotalItemCount);
        }

        /// <summary>One query for the page, so the log can offer the administrator override.</summary>
        private async Task AttachLiveRequestsAsync(IReadOnlyList<VacationEventDto> rows, int organizationId)
        {
            if (rows.Count == 0)
            {
                return;
            }

            var today = await _organizationDbSet.TodayAsync(organizationId);
            var ids = rows.Select(row => row.RequestId).Distinct().ToList();

            var requests = await _requestDbSet
                .AsNoTracking()
                .Include(request => request.Employee)
                .Include(request => request.ReviewedBy)
                .Where(request => request.OrganizationId == organizationId && ids.Contains(request.Id))
                .ToListAsync();

            var byId = requests.ToDictionary(
                request => request.Id,
                request => VacationMapper.ToRequest(request, today));

            foreach (var row in rows)
            {
                row.Request = byId.TryGetValue(row.RequestId, out var request) ? request : null;
            }
        }

        private static IQueryable<VacationRequestEvent> ApplySort(IQueryable<VacationRequestEvent> query, VacationLogArgsDto args)
        {
            var descending = !string.Equals(args.Dir, "asc", StringComparison.OrdinalIgnoreCase);

            // Id breaks ties outside the direction: events sharing a timestamp
            // would otherwise reverse, showing an approval before its submission.
            return (args.Sort ?? string.Empty).Trim() switch
            {
                "employee" => descending
                    ? query.OrderByDescending(e => e.Employee.FirstName).ThenByDescending(e => e.Employee.LastName).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Employee.FirstName).ThenBy(e => e.Employee.LastName).ThenBy(e => e.Id),
                "kind" => descending
                    ? query.OrderByDescending(e => e.Kind).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Kind).ThenBy(e => e.Id),
                "actor" => descending
                    ? query.OrderByDescending(e => e.Actor.FirstName).ThenByDescending(e => e.Actor.LastName).ThenBy(e => e.Id)
                    : query.OrderBy(e => e.Actor.FirstName).ThenBy(e => e.Actor.LastName).ThenBy(e => e.Id),
                _ => descending
                    ? query.OrderByDescending(e => e.OccurredAt).ThenByDescending(e => e.Id)
                    : query.OrderBy(e => e.OccurredAt).ThenBy(e => e.Id)
            };
        }
    }
}
