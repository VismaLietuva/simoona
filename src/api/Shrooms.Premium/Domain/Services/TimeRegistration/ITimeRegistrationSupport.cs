using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimeRegistrationSupport
    {
        Task<bool> IsAccountantAsync(UserAndOrganizationDto viewer);

        Task<TimeActor> ResolveAsync(UserAndOrganizationDto viewer, string employeeId);

        Task<DateTime> TodayAsync(int organizationId);

        Task<IReadOnlyDictionary<DateTime, string>> HolidaysAsync(DateTime month);

        Task<IDictionary<string, int>> DailyMinutesAsync(IReadOnlyCollection<string> userIds);

        Task<TimeCalendar> CalendarAsync(string userId, DateTime month, int organizationId);

        Task<TimeMonthLock> LockAsync(DateTime month, int organizationId);

        Task ThrowIfLockedAsync(DateTime month, int organizationId);

        Task<TimePeriod> PeriodAsync(string userId, DateTime month, int organizationId);

        Task<TimePeriodStatus> StatusAsync(string userId, DateTime month, int organizationId);

        Task<IDictionary<string, TimePeriodStatus>> StatusesAsync(IReadOnlyCollection<string> userIds, DateTime month, int organizationId);

        Task SetStatusAsync(string userId, DateTime month, int organizationId, TimePeriodStatus status, TimePeriodStatus? previous);

        Task<int?> AutoFillProjectIdAsync(string userId, int organizationId);

        Task MaterializeAutoFillAsync(string userId, DateTime month, int organizationId);

        Task<IReadOnlyList<string>> EmployeeIdsAsync(int organizationId);

        Task<IReadOnlyList<string>> ProjectMemberIdsAsync(int organizationId);
    }
}
