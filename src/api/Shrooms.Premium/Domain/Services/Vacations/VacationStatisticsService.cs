using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationStatisticsService : IVacationStatisticsService
    {
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;
        private readonly IHolidayService _holidayService;

        public VacationStatisticsService(IUnitOfWork2 uow, IHolidayService holidayService)
        {
            _holidayService = holidayService;
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<VacationStatisticsDto> GetStatisticsAsync(VacationStatisticsArgsDto args)
        {
            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == args.OrganizationId);

            var today = VacationCalculator.TodayIn(organization?.TimeZone);

            var userQuery = _userDbSet
                .AsNoTracking()
                .Where(user => user.OrganizationId == args.OrganizationId);

            if (!string.IsNullOrWhiteSpace(args.Search))
            {
                var term = args.Search.Trim();
                userQuery = userQuery.Where(user => user.FirstName.Contains(term) || user.LastName.Contains(term));
            }

            var users = await userQuery.ToListAsync();
            var userIds = users.Select(user => user.Id).ToList();

            // One pass over every request belonging to the people in scope. The
            // six figures below all read from it; querying per employee turned a
            // 200-person table into 200 round trips.
            var requests = await _requestDbSet
                .AsNoTracking()
                .Where(request => request.OrganizationId == args.OrganizationId && userIds.Contains(request.EmployeeId))
                .ToListAsync();

            var byEmployee = requests
                .GroupBy(request => request.EmployeeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var holidays = await _holidayService.GetCalendarAsync();

            var rows = users
                .Select(user => BuildRow(user, byEmployee, today, holidays))
                .ToList();

            rows = ApplySort(rows, args).ToList();

            var latestPayslip = users
                .Select(user => user.VacationLastTimeUpdated)
                .Where(date => date.HasValue)
                .DefaultIfEmpty(null)
                .Max();

            return new VacationStatisticsDto
            {
                Rows = rows,
                BalanceAsOf = VacationWireFormat.ToDay(latestPayslip?.Date),
                Totals = new VacationStatsTotalsDto
                {
                    Accrued = rows.Sum(row => row.Accrued),
                    Booked = rows.Sum(row => row.Booked),
                    Remaining = rows.Sum(row => row.Remaining),
                    Taken = rows.Sum(row => row.Taken),
                    Upcoming = rows.Sum(row => row.Upcoming),
                    PendingCount = rows.Sum(row => row.PendingCount)
                }
            };
        }

        private static VacationStatsDto BuildRow(
            ApplicationUser user,
            IReadOnlyDictionary<string, List<VacationRequest>> byEmployee,
            DateTime today,
            HolidayCalendar holidays)
        {
            var own = byEmployee.TryGetValue(user.Id, out var list) ? list : new List<VacationRequest>();

            var accrued = user.VacationUnusedTime ?? 0;
            var booked = VacationCalculator.CommittedAnnualDays(own, user.VacationLastTimeUpdated?.Date, holidays);

            // taken + upcoming deliberately does not equal booked: booked also
            // carries pending requests and starts at the payslip cutoff, whereas
            // these two describe the whole approved history either side of today.
            // Leave in progress right now falls into neither.
            double taken = 0;
            double upcoming = 0;

            foreach (var request in own)
            {
                if (request.Status != VacationRequestStatus.Approved || !VacationCalculator.DeductsBalance(request.Type))
                {
                    continue;
                }

                if (request.DateTo.Date < today)
                {
                    taken += request.WorkingDays;
                }
                else if (request.DateFrom.Date > today)
                {
                    upcoming += request.WorkingDays;
                }
            }

            return new VacationStatsDto
            {
                Employee = VacationMapper.ToPerson(user),
                Accrued = accrued,
                Booked = booked,
                Remaining = accrued - booked,
                Taken = taken,
                Upcoming = upcoming,
                // Any type, not just annual: a pending parental day is still a
                // decision somebody owes an answer on.
                PendingCount = own.Count(request => request.Status == VacationRequestStatus.Pending),
                YearsOfService = user.YearsEmployed
            };
        }

        private static IEnumerable<VacationStatsDto> ApplySort(List<VacationStatsDto> rows, VacationStatisticsArgsDto args)
        {
            var descending = string.Equals(args.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            Func<VacationStatsDto, IComparable> key = (args.Sort ?? string.Empty).Trim() switch
            {
                "accrued" => row => row.Accrued,
                "booked" => row => row.Booked,
                "remaining" => row => row.Remaining,
                "taken" => row => row.Taken,
                "upcoming" => row => row.Upcoming,
                "pendingCount" => row => row.PendingCount,
                _ => row => row.Employee.FullName
            };

            // Name is the tie-break so equal figures keep a readable order, and
            // it sits outside the direction like every other listing here.
            return descending
                ? rows.OrderByDescending(key).ThenBy(row => row.Employee.FullName)
                : rows.OrderBy(key).ThenBy(row => row.Employee.FullName);
        }
    }
}
