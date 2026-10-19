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
        private readonly DbSet<TimeTask> _tasks;
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
            _tasks = uow.GetDbSet<TimeTask>();
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
            var userId = string.IsNullOrWhiteSpace(employeeId) ? viewer.UserId : employeeId;

            if (!TimeRegistrationRules.SameUser(userId, viewer.UserId))
            {
                var target = await _users
                    .AsNoTracking()
                    .Where(user => user.OrganizationId == viewer.OrganizationId && user.Id == userId)
                    .Select(user => new { user.ManagerId })
                    .FirstOrDefaultAsync();

                if (target == null || (!isAccountant && !TimeRegistrationRules.SameUser(target.ManagerId, viewer.UserId)))
                {
                    throw new UnauthorizedException();
                }
            }

            return new TimeActor(viewer, userId, isAccountant);
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
            var holidays = await HolidaysAsync(month);
            var daily = (await DailyMinutesAsync(new[] { userId }))[userId];
            var absences = await AbsencesAsync(userId, month, holidays, organizationId);
            var employedFrom = await _users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => user.EmploymentDate)
                .FirstOrDefaultAsync();

            return new TimeCalendar(month, holidays, absences, daily, employedFrom);
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
            var usable = await _members
                .AsNoTracking()
                .Where(member => member.OrganizationId == organizationId
                                 && member.UserId == userId
                                 && !member.Project.IsDeleted
                                 && !member.Project.IsArchived
                                 && member.Project.Tasks.Any(task => task.IsDefault && !task.IsDeleted))
                .Select(member => new
                {
                    member.ProjectId,
                    member.IsAutoFill,
                    CustomerName = member.Project.Customer.Name,
                    ProjectName = member.Project.Name
                })
                .ToListAsync();

            var chosen = usable.FirstOrDefault(member => member.IsAutoFill)
                         ?? usable
                             .OrderBy(member => member.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                             .ThenBy(member => member.ProjectName, StringComparer.CurrentCultureIgnoreCase)
                             .FirstOrDefault();

            return chosen?.ProjectId;
        }

        public async Task MaterializeAutoFillAsync(string userId, DateTime month, int organizationId)
        {
            var projectId = await AutoFillProjectIdAsync(userId, organizationId);
            if (projectId == null)
            {
                return;
            }

            var taskId = await _tasks
                .AsNoTracking()
                .Where(task => task.ProjectId == projectId && task.IsDefault && !task.IsDeleted)
                .Select(task => (int?)task.Id)
                .FirstOrDefaultAsync();
            if (taskId == null)
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
                if (!calendar.IsWorkingDay(day) || calendar.Absences.ContainsKey(day))
                {
                    continue;
                }

                var onDay = logged.Where(entry => entry.Date.Date == day).ToList();
                if (onDay.Any(entry => entry.TaskId == taskId))
                {
                    continue;
                }

                var rest = calendar.DailyMinutes - onDay.Sum(entry => entry.Minutes);
                if (rest > 0)
                {
                    _entries.Add(new TimeEntry
                    {
                        OrganizationId = organizationId,
                        UserId = userId,
                        ProjectId = projectId.Value,
                        TaskId = taskId.Value,
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

        private async Task<IReadOnlyDictionary<DateTime, TimeAbsence>> AbsencesAsync(
            string userId,
            DateTime month,
            IReadOnlyDictionary<DateTime, string> holidays,
            int organizationId)
        {
            var last = TimeMonth.LastDay(month);
            var requests = await _vacations
                .AsNoTracking()
                .Where(request => request.OrganizationId == organizationId
                                  && request.EmployeeId == userId
                                  && (request.Status == VacationRequestStatus.Pending || request.Status == VacationRequestStatus.Approved)
                                  && request.DateFrom <= last
                                  && request.DateTo >= month)
                .Select(request => new { request.Type, request.Status, request.DateFrom, request.DateTo })
                .ToListAsync();

            var absences = new Dictionary<DateTime, TimeAbsence>();
            foreach (var request in requests)
            {
                for (var day = request.DateFrom.Date; day <= request.DateTo.Date; day = day.AddDays(1))
                {
                    if (day < month || day > last || TimeMonth.IsWeekend(day) || holidays.ContainsKey(day))
                    {
                        continue;
                    }

                    if (!absences.TryGetValue(day, out var current) || current.Status != VacationRequestStatus.Approved)
                    {
                        absences[day] = new TimeAbsence(day, request.Type, request.Status);
                    }
                }
            }

            return absences;
        }
    }
}
