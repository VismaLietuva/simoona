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
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimePeriodService : ITimePeriodService
    {
        private static readonly Dictionary<TimePeriodStatus, int> StatusOrder = new Dictionary<TimePeriodStatus, int>
        {
            [TimePeriodStatus.Submitted] = 0,
            [TimePeriodStatus.Open] = 1,
            [TimePeriodStatus.Approved] = 2
        };

        private readonly IUnitOfWork2 _uow;
        private readonly ITimeRegistrationSupport _support;
        private readonly DbSet<ApplicationUser> _users;
        private readonly DbSet<TimeMonthLock> _locks;

        public TimePeriodService(IUnitOfWork2 uow, ITimeRegistrationSupport support)
        {
            _uow = uow;
            _support = support;
            _users = uow.GetDbSet<ApplicationUser>();
            _locks = uow.GetDbSet<TimeMonthLock>();
        }

        public async Task SubmitAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            await _support.ThrowIfLockedAsync(first, userOrg.OrganizationId);

            if (await _support.StatusAsync(userOrg.UserId, first, userOrg.OrganizationId) != TimePeriodStatus.Open)
            {
                throw WrongStatus("This month is already submitted");
            }

            if (first > TimeMonth.Of(await _support.TodayAsync(userOrg.OrganizationId)))
            {
                throw WrongStatus("A future month cannot be submitted");
            }

            await _support.MaterializeAutoFillAsync(userOrg.UserId, first, userOrg.OrganizationId);
            await _support.SetStatusAsync(userOrg.UserId, first, userOrg.OrganizationId, TimePeriodStatus.Submitted, null);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task WithdrawAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            await _support.ThrowIfLockedAsync(first, userOrg.OrganizationId);

            if (first < TimeMonth.Of(await _support.TodayAsync(userOrg.OrganizationId)))
            {
                throw WrongStatus("A past month cannot be withdrawn");
            }

            if (await _support.StatusAsync(userOrg.UserId, first, userOrg.OrganizationId) != TimePeriodStatus.Submitted)
            {
                throw WrongStatus("Only a submitted month can be withdrawn");
            }

            await _support.SetStatusAsync(userOrg.UserId, first, userOrg.OrganizationId, TimePeriodStatus.Open, null);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task ApproveAsync(string employeeId, string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            var actor = await ResolveOtherAsync(userOrg, employeeId, "You cannot approve your own month");
            await _support.ThrowIfLockedAsync(first, actor.OrganizationId);

            var status = await _support.StatusAsync(actor.UserId, first, actor.OrganizationId);
            if (status == TimePeriodStatus.Approved)
            {
                throw WrongStatus("This month is already approved");
            }

            if (first > TimeMonth.Of(await _support.TodayAsync(actor.OrganizationId)))
            {
                throw WrongStatus("A future month cannot be approved");
            }

            await _support.MaterializeAutoFillAsync(actor.UserId, first, actor.OrganizationId);
            await _support.SetStatusAsync(actor.UserId, first, actor.OrganizationId, TimePeriodStatus.Approved, status);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task UnapproveAsync(string employeeId, string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            var actor = await ResolveOtherAsync(userOrg, employeeId, "You cannot unapprove your own month");
            await _support.ThrowIfLockedAsync(first, actor.OrganizationId);

            if (first < TimeMonth.Of(await _support.TodayAsync(actor.OrganizationId)) && !actor.IsAccountant)
            {
                throw WrongStatus("Only an administrator can unapprove a past month");
            }

            var period = await _support.PeriodAsync(actor.UserId, first, actor.OrganizationId);
            if (period?.Status != TimePeriodStatus.Approved)
            {
                throw WrongStatus("Only an approved month can be unapproved");
            }

            period.Status = period.PreviousStatus ?? TimePeriodStatus.Submitted;
            period.PreviousStatus = null;
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task LockAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            await ThrowIfNotAccountantAsync(userOrg);

            if (first > TimeMonth.Of(await _support.TodayAsync(userOrg.OrganizationId)))
            {
                throw WrongStatus("A future month cannot be locked");
            }

            await _support.ThrowIfLockedAsync(first, userOrg.OrganizationId);

            var employeeIds = await _support.ProjectMemberIdsAsync(userOrg.OrganizationId);
            var statuses = await _support.StatusesAsync(employeeIds.ToList(), first, userOrg.OrganizationId);
            foreach (var employeeId in employeeIds.Where(id => statuses[id] == TimePeriodStatus.Open))
            {
                await _support.MaterializeAutoFillAsync(employeeId, first, userOrg.OrganizationId);
            }

            _locks.Add(new TimeMonthLock { OrganizationId = userOrg.OrganizationId, Month = first, LockedById = userOrg.UserId });
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task UnlockAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            await ThrowIfNotAccountantAsync(userOrg);

            var monthLock = await _locks
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Month == first)
                ?? throw WrongStatus("This month is not locked");

            _locks.Remove(monthLock);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<IEnumerable<TimeTeamMemberDto>> GetTeamAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            var ids = await DirectReportIdsAsync(userOrg);
            return await MembersAsync(ids, first, userOrg.OrganizationId);
        }

        public async Task<IEnumerable<TimeTeamMemberDto>> GetEmployeesAsync(string month, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            await ThrowIfNotAccountantAsync(userOrg);

            var ids = (await _support.EmployeeIdsAsync(userOrg.OrganizationId))
                .Where(id => !TimeRegistrationRules.SameUser(id, userOrg.UserId))
                .ToList();
            return await MembersAsync(ids, first, userOrg.OrganizationId);
        }

        public async Task<TimeManagerDto> GetManagerAsync(string employeeId, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, employeeId);

            return await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == actor.OrganizationId && user.Id == actor.UserId && user.Manager != null)
                .Select(user => new TimeManagerDto
                {
                    Id = user.Manager.Id,
                    FullName = ((user.Manager.FirstName ?? string.Empty) + " " + (user.Manager.LastName ?? string.Empty)).Trim(),
                    PictureId = user.Manager.PictureId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TimeRegistrationOverviewDto> GetOverviewAsync(UserAndOrganizationDto userOrg)
        {
            var reports = await DirectReportIdsAsync(userOrg);
            var overview = new TimeRegistrationOverviewDto { CanManage = reports.Count > 0 };

            var today = await _support.TodayAsync(userOrg.OrganizationId);
            var month = TimeMonth.Of(today);
            if (await _locks.AnyAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Month == month))
            {
                return overview;
            }

            if (today < await ReminderStartAsync(userOrg, month))
            {
                return overview;
            }

            overview.ManageBadge = await NotApprovedAsync(reports, month, userOrg.OrganizationId);

            if (await _support.IsAccountantAsync(userOrg))
            {
                var employees = (await _support.EmployeeIdsAsync(userOrg.OrganizationId))
                    .Where(id => !TimeRegistrationRules.SameUser(id, userOrg.UserId))
                    .ToList();
                overview.AdministrateBadge = await NotApprovedAsync(employees, month, userOrg.OrganizationId);
            }

            return overview;
        }

        private async Task<DateTime> ReminderStartAsync(UserAndOrganizationDto userOrg, DateTime month)
        {
            var calendar = await _support.CalendarAsync(userOrg.UserId, month, userOrg.OrganizationId);
            var working = calendar.Days
                .Where(day => calendar.IsWorkingDay(day) && !calendar.Absences.ContainsKey(day))
                .ToList();

            return working.Count > TimeRegistrationRules.ReminderWorkingDays
                ? working[working.Count - 1 - TimeRegistrationRules.ReminderWorkingDays]
                : month;
        }

        private async Task<int> NotApprovedAsync(IReadOnlyCollection<string> userIds, DateTime month, int organizationId)
        {
            if (userIds.Count == 0)
            {
                return 0;
            }

            var statuses = await _support.StatusesAsync(userIds, month, organizationId);
            return statuses.Values.Count(status => status != TimePeriodStatus.Approved);
        }

        private async Task<List<string>> DirectReportIdsAsync(UserAndOrganizationDto userOrg)
        {
            return await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == userOrg.OrganizationId && user.ManagerId == userOrg.UserId)
                .Select(user => user.Id)
                .ToListAsync();
        }

        private async Task<IEnumerable<TimeTeamMemberDto>> MembersAsync(IReadOnlyCollection<string> ids, DateTime month, int organizationId)
        {
            if (ids.Count == 0)
            {
                return new List<TimeTeamMemberDto>();
            }

            var users = await _users
                .AsNoTracking()
                .Where(user => user.OrganizationId == organizationId && ids.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    JobTitle = user.JobPosition != null ? user.JobPosition.Title : null,
                    user.PictureId
                })
                .ToListAsync();
            var statuses = await _support.StatusesAsync(ids, month, organizationId);
            var daily = await _support.DailyMinutesAsync(ids);

            return users
                .Select(user => new
                {
                    Dto = new TimeTeamMemberDto
                    {
                        Id = user.Id,
                        FullName = TimeTableService.FullName(user.FirstName, user.LastName),
                        Email = user.Email,
                        JobTitle = user.JobTitle,
                        PictureId = user.PictureId,
                        Status = TimeTableService.Wire(statuses[user.Id]),
                        DailyMinutes = daily[user.Id]
                    },
                    Status = statuses[user.Id]
                })
                .OrderBy(row => StatusOrder[row.Status])
                .ThenBy(row => row.Dto.FullName, StringComparer.CurrentCultureIgnoreCase)
                .Select(row => row.Dto)
                .ToList();
        }

        private async Task<TimeActor> ResolveOtherAsync(UserAndOrganizationDto userOrg, string employeeId, string selfMessage)
        {
            if (string.IsNullOrWhiteSpace(employeeId) || TimeRegistrationRules.SameUser(employeeId, userOrg.UserId))
            {
                throw WrongStatus(selfMessage);
            }

            return await _support.ResolveAsync(userOrg, employeeId);
        }

        private async Task ThrowIfNotAccountantAsync(UserAndOrganizationDto userOrg)
        {
            if (!await _support.IsAccountantAsync(userOrg))
            {
                throw new UnauthorizedException();
            }
        }

        private static ValidationException WrongStatus(string message)
        {
            return new ValidationException(ErrorCodes.TimeRegistrationWrongStatus, message);
        }
    }
}
