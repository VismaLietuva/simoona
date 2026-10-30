using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeRegistrationSupport : ITimeRegistrationSupport
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly ISystemClock _clock;
        private readonly IHolidayService _holidayService;
        private readonly DbSet<ApplicationUser> _users;
        private readonly DbSet<Organization> _organizations;
        private readonly DbSet<VacationRequest> _vacations;
        private readonly DbSet<TimeProjectMember> _members;
        private readonly DbSet<TimeEntry> _entries;
        private readonly DbSet<TimePeriod> _periods;
        private readonly DbSet<TimeMonthLock> _locks;

        public TimeRegistrationSupport(
            IUnitOfWork2 uow,
            IPermissionService permissionService,
            IRoleService roleService,
            IHolidayService holidayService,
            ISystemClock clock)
        {
            _permissionService = permissionService;
            _roleService = roleService;
            _clock = clock;
            _holidayService = holidayService;
            _users = uow.GetDbSet<ApplicationUser>();
            _organizations = uow.GetDbSet<Organization>();
            _vacations = uow.GetDbSet<VacationRequest>();
            _members = uow.GetDbSet<TimeProjectMember>();
            _entries = uow.GetDbSet<TimeEntry>();
            _periods = uow.GetDbSet<TimePeriod>();
            _locks = uow.GetDbSet<TimeMonthLock>();
        }

        public Task<bool> IsAccountantAsync(UserAndOrganizationDto viewer)
        {
            return _permissionService.UserHasPermissionAsync(viewer, AdministrationPermissions.TimeRegistration);
        }

        public Task InUserMonthAsync(int organizationId, DateTime month, string userId, Func<Task> work)
        {
            var monthKey = MonthKey(organizationId, month);
            return SerializedAsync(work, (monthKey, "Shared"), ($"{monthKey}:{userId.ToLowerInvariant()}", "Exclusive"));
        }

        public Task InMonthAsync(int organizationId, DateTime month, Func<Task> work)
        {
            return SerializedAsync(work, (MonthKey(organizationId, month), "Exclusive"));
        }

        public async Task<TimeActor> ResolveAsync(UserAndOrganizationDto viewer, string employeeId)
        {
            var isAccountant = await IsAccountantAsync(viewer);
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return new TimeActor(viewer, viewer.UserId, isAccountant);
            }

            var target = await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == viewer.OrganizationId && user.Id == employeeId)
                .Select(user => new { user.Id, user.ManagerId })
                .FirstOrDefaultAsync() ?? throw new UnauthorizedException();

            if (TimeRegistrationRules.SameUser(target.Id, viewer.UserId))
            {
                return new TimeActor(viewer, viewer.UserId, isAccountant);
            }

            if (!isAccountant && !TimeRegistrationRules.SameUser(target.ManagerId, viewer.UserId))
            {
                throw new UnauthorizedException();
            }

            return new TimeActor(viewer, target.Id, isAccountant);
        }

        public async Task<DateTime> TodayAsync(int organizationId)
        {
            return VacationCalculator.NowIn(await _organizations.TimeZoneAsync(organizationId), _clock.UtcNow).Date;
        }

        public async Task<IReadOnlyDictionary<DateTime, string>> HolidaysAsync(DateTime month)
        {
            var last = TimeMonth.LastDay(month);
            var holidays = (await _holidayService.GetAsync(month, last))
                .GroupBy(holiday => holiday.Date.Date)
                .ToDictionary(group => group.Key, group => group.First().Name);

            foreach (var (date, name) in LithuanianWeekendHolidays.For(month.Year))
            {
                if (date >= month && date <= last && !holidays.ContainsKey(date))
                {
                    holidays[date] = name;
                }
            }

            return holidays;
        }

        public async Task<IDictionary<string, int>> DailyMinutesAsync(IReadOnlyCollection<string> userIds)
        {
            var hours = await _users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    HasHours = user.WorkingHours != null,
                    FullTime = user.WorkingHours != null && user.WorkingHours.FullTime,
                    PartTimeHours = user.WorkingHours != null ? user.WorkingHours.PartTimeHours : null
                })
                .ToListAsync();

            return userIds.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(
                id => id,
                id =>
                {
                    var row = hours.FirstOrDefault(user => TimeRegistrationRules.SameUser(user.Id, id));
                    return row == null
                        ? TimeRegistrationRules.FullDayMinutes
                        : TimeRegistrationRules.DailyMinutes(row.HasHours, row.FullTime, row.PartTimeHours);
                },
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<TimeCalendar> CalendarAsync(string userId, DateTime month, int organizationId)
        {
            return (await CalendarsAsync(new[] { userId }, month, organizationId))[userId];
        }

        public async Task<IDictionary<string, TimeCalendar>> CalendarsAsync(IReadOnlyCollection<string> userIds, DateTime month, int organizationId)
        {
            var holidays = await HolidaysAsync(month);
            var daily = await DailyMinutesAsync(userIds);
            var absences = await AbsencesAsync(userIds, month, holidays, organizationId);
            var employment = (await _users
                    .AsNoTracking()
                    .Where(user => userIds.Contains(user.Id) && user.EmploymentDate != null)
                    .Select(user => new { user.Id, user.EmploymentDate })
                    .ToListAsync())
                .ToDictionary(user => user.Id, user => user.EmploymentDate.Value, StringComparer.OrdinalIgnoreCase);
            var timeZone = employment.Count > 0 ? await _organizations.TimeZoneAsync(organizationId) : null;

            return userIds.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(
                id => id,
                id => new TimeCalendar(
                    month,
                    holidays,
                    absences.TryGetValue(id, out var own) ? own : new Dictionary<DateTime, TimeAbsence>(),
                    daily[id],
                    employment.TryGetValue(id, out var employedFrom) ? VacationCalculator.NowIn(timeZone, employedFrom) : null),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<TimeMonthLock> LockAsync(DateTime month, int organizationId)
        {
            return await _locks
                .IgnoreQueryFilters()
                .Include(monthLock => monthLock.LockedBy)
                .FirstOrDefaultAsync(monthLock => monthLock.OrganizationId == organizationId && monthLock.Month == month);
        }

        public async Task ThrowIfLockedAsync(DateTime month, int organizationId)
        {
            if (await _locks.AnyAsync(monthLock => monthLock.OrganizationId == organizationId && monthLock.Month == month))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationLocked, "This month is locked");
            }
        }

        public async Task<TimePeriod> PeriodAsync(string userId, DateTime month, int organizationId)
        {
            return await _periods
                .FirstOrDefaultAsync(period => period.OrganizationId == organizationId && period.UserId == userId && period.Month == month);
        }

        public async Task<TimePeriodStatus> StatusAsync(string userId, DateTime month, int organizationId)
        {
            return (await PeriodAsync(userId, month, organizationId))?.Status ?? TimePeriodStatus.Open;
        }

        public async Task<IDictionary<string, TimePeriodStatus>> StatusesAsync(IReadOnlyCollection<string> userIds, DateTime month, int organizationId)
        {
            var periods = await _periods
                .AsNoTracking()
                .Where(period => period.OrganizationId == organizationId && period.Month == month && userIds.Contains(period.UserId))
                .Select(period => new { period.UserId, period.Status })
                .ToListAsync();

            return userIds.Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(
                id => id,
                id => periods.FirstOrDefault(period => TimeRegistrationRules.SameUser(period.UserId, id))?.Status ?? TimePeriodStatus.Open,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task SetStatusAsync(string userId, DateTime month, int organizationId, TimePeriodStatus status, TimePeriodStatus? previous)
        {
            var period = await PeriodAsync(userId, month, organizationId);
            if (period == null)
            {
                _periods.Add(new TimePeriod
                {
                    OrganizationId = organizationId,
                    UserId = userId,
                    Month = month,
                    Status = status,
                    PreviousStatus = previous
                });
                return;
            }

            period.Status = status;
            period.PreviousStatus = previous;
        }

        public async Task<int?> AutoFillProjectIdAsync(string userId, int organizationId)
        {
            return (await AutoFillsAsync(new[] { userId }, organizationId)).TryGetValue(userId, out var autoFill)
                ? autoFill.ProjectId
                : null;
        }

        public async Task<IDictionary<string, TimeAutoFill>> AutoFillsAsync(IReadOnlyCollection<string> userIds, int organizationId)
        {
            var usable = await _members
                .AsNoTracking()
                .Where(member => member.OrganizationId == organizationId
                                 && userIds.Contains(member.UserId)
                                 && !member.Project.IsDeleted
                                 && !member.Project.IsArchived
                                 && member.Project.Tasks.Any(task => task.IsDefault && !task.IsDeleted))
                .Select(member => new
                {
                    member.UserId,
                    member.ProjectId,
                    member.IsAutoFill,
                    CustomerName = member.Project.Customer.Name,
                    ProjectName = member.Project.Name,
                    Task = member.Project.Tasks
                        .Where(task => task.IsDefault && !task.IsDeleted)
                        .Select(task => new { task.Id, task.Name })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return usable
                .GroupBy(member => member.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var chosen = group.FirstOrDefault(member => member.IsAutoFill)
                                     ?? group
                                         .OrderBy(member => member.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                                         .ThenBy(member => member.ProjectName, StringComparer.CurrentCultureIgnoreCase)
                                         .First();
                        return new TimeAutoFill(chosen.ProjectId, chosen.Task.Id, chosen.CustomerName, chosen.ProjectName, chosen.Task.Name);
                    },
                    StringComparer.OrdinalIgnoreCase);
        }

        public async Task MaterializeAutoFillAsync(string userId, DateTime month, int organizationId)
        {
            if (!(await AutoFillsAsync(new[] { userId }, organizationId)).TryGetValue(userId, out var autoFill))
            {
                return;
            }

            var calendar = await CalendarAsync(userId, month, organizationId);
            var last = TimeMonth.LastDay(month);
            var logged = await _entries
                .AsNoTracking()
                .Where(entry => entry.OrganizationId == organizationId && entry.UserId == userId && entry.Date >= month && entry.Date <= last)
                .Select(entry => new { entry.Date, entry.TaskId, entry.Minutes })
                .ToListAsync();

            foreach (var day in calendar.Days)
            {
                var onDay = logged.Where(entry => entry.Date.Date == day).ToList();
                var rest = calendar.AutoFillMinutes(day, onDay.Sum(entry => entry.Minutes), onDay.Any(entry => entry.TaskId == autoFill.TaskId));
                if (rest > 0)
                {
                    _entries.Add(new TimeEntry
                    {
                        OrganizationId = organizationId,
                        UserId = userId,
                        ProjectId = autoFill.ProjectId,
                        TaskId = autoFill.TaskId,
                        Date = day,
                        Minutes = rest
                    });
                }
            }
        }

        public async Task<IReadOnlyList<string>> EmployeeIdsAsync(int organizationId)
        {
            return await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == organizationId)
                .Where(_roleService.ExcludeUsersWithRoleName(Roles.NewUser))
                .Select(user => user.Id)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> ProjectMemberIdsAsync(int organizationId)
        {
            var members = await _members
                .AsNoTracking()
                .Where(member => member.OrganizationId == organizationId && !member.Project.IsDeleted && !member.Project.IsArchived)
                .Select(member => member.UserId)
                .Distinct()
                .ToListAsync();

            return await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == organizationId && members.Contains(user.Id))
                .Select(user => user.Id)
                .ToListAsync();
        }

        private static string MonthKey(int organizationId, DateTime month)
        {
            return $"TimeRegistration:{organizationId}:{TimeMonth.ToText(month)}";
        }

        private async Task SerializedAsync(Func<Task> work, params (string Resource, string Mode)[] appLocks)
        {
            var database = _entries.GetService<ICurrentDbContext>().Context.Database;
            if (!database.IsRelational())
            {
                await work();
                return;
            }

            await using var transaction = await database.BeginTransactionAsync();
            foreach (var (resource, mode) in appLocks)
            {
                var result = new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                await database.ExecuteSqlRawAsync(
                    "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = @mode, @LockOwner = 'Transaction', @LockTimeout = 15000",
                    result,
                    new SqlParameter("@resource", resource),
                    new SqlParameter("@mode", mode));

                if ((int)result.Value < 0)
                {
                    throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, "Another change is in progress. Try again");
                }
            }

            await work();
            await transaction.CommitAsync();
        }

        private async Task<IDictionary<string, Dictionary<DateTime, TimeAbsence>>> AbsencesAsync(
            IReadOnlyCollection<string> userIds,
            DateTime month,
            IReadOnlyDictionary<DateTime, string> holidays,
            int organizationId)
        {
            var last = TimeMonth.LastDay(month);
            var requests = await _vacations
                .AsNoTracking()
                .Where(request => request.OrganizationId == organizationId
                                  && userIds.Contains(request.EmployeeId)
                                  && (request.Status == VacationRequestStatus.Pending || request.Status == VacationRequestStatus.Approved)
                                  && request.DateFrom <= last
                                  && request.DateTo >= month)
                .Select(request => new { request.EmployeeId, request.Type, request.Status, request.DateFrom, request.DateTo })
                .ToListAsync();

            var absences = new Dictionary<string, Dictionary<DateTime, TimeAbsence>>(StringComparer.OrdinalIgnoreCase);
            foreach (var request in requests)
            {
                if (!absences.TryGetValue(request.EmployeeId, out var own))
                {
                    own = new Dictionary<DateTime, TimeAbsence>();
                    absences[request.EmployeeId] = own;
                }

                for (var day = request.DateFrom.Date; day <= request.DateTo.Date; day = day.AddDays(1))
                {
                    if (day < month || day > last || TimeMonth.IsWeekend(day) || holidays.ContainsKey(day))
                    {
                        continue;
                    }

                    if (!own.TryGetValue(day, out var current) || current.Status != VacationRequestStatus.Approved)
                    {
                        own[day] = new TimeAbsence(day, request.Type, request.Status);
                    }
                }
            }

            return absences;
        }
    }
}
