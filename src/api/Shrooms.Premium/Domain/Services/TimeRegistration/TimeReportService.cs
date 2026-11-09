using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeReportService : ITimeReportService
    {
        private readonly ITimeRegistrationSupport _support;
        private readonly DbSet<ApplicationUser> _users;
        private readonly DbSet<TimeEntry> _entries;

        public TimeReportService(IUnitOfWork2 uow, ITimeRegistrationSupport support)
        {
            _support = support;
            _users = uow.GetDbSet<ApplicationUser>();
            _entries = uow.GetDbSet<TimeEntry>();
        }

        public async Task<TimeReportDto> GetReportAsync(string month, UserAndOrganizationDto userOrg)
        {
            if (!await _support.IsAccountantAsync(userOrg))
            {
                throw new UnauthorizedException();
            }

            var first = TimeMonth.Parse(month);
            var last = TimeMonth.LastDay(first);
            var organizationId = userOrg.OrganizationId;
            var activeIds = await _support.EmployeeIdsAsync(organizationId);
            var active = new HashSet<string>(activeIds, StringComparer.OrdinalIgnoreCase);

            var entries = (await _entries
                    .AsNoTracking()
                    .Where(entry => entry.OrganizationId == organizationId && entry.Date >= first && entry.Date <= last)
                    .Select(entry => new ReportEntry
                    {
                        UserId = entry.UserId,
                        ProjectId = entry.ProjectId,
                        TaskId = entry.TaskId,
                        Date = entry.Date,
                        Minutes = entry.Minutes,
                        CustomerName = entry.Project.Customer.Name,
                        ProjectName = entry.Project.Name,
                        TaskName = entry.Task.Name,
                        IsOvertime = entry.Task.IsOvertime
                    })
                    .ToListAsync())
                .ToLookup(entry => entry.UserId, StringComparer.OrdinalIgnoreCase);
            var ids = activeIds
                .Concat(entries.Select(group => group.Key).Where(id => !active.Contains(id)))
                .ToList();

            var users = await _users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(user => user.OrganizationId == organizationId && ids.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    JobTitle = user.JobPosition != null ? user.JobPosition.Title : null,
                    user.PictureId,
                    ManagerFirstName = user.Manager.FirstName,
                    ManagerLastName = user.Manager.LastName
                })
                .ToListAsync();
            var statuses = await _support.StatusesAsync(ids, first, organizationId);
            var calendars = await _support.CalendarsAsync(activeIds, first, organizationId);
            var autoFillPending = first <= TimeMonth.Of(await _support.TodayAsync(organizationId))
                                  && await _support.LockAsync(first, organizationId) == null;
            var autoFills = autoFillPending
                ? await _support.AutoFillsAsync(activeIds, organizationId)
                : new Dictionary<string, TimeAutoFill>();

            var employees = new List<TimeReportEmployeeDto>();
            var projects = new List<TimeReportProjectDto>();
            var absences = new List<TimeReportProjectDto>();
            foreach (var user in users)
            {
                var isFormer = !active.Contains(user.Id);
                var calendar = isFormer ? null : calendars[user.Id];
                var status = statuses[user.Id];
                var rows = new Dictionary<(int ProjectId, int TaskId), TimeReportProjectDto>();
                foreach (var entry in entries[user.Id])
                {
                    Row(rows, user.Id, entry.ProjectId, entry.TaskId, entry.CustomerName, entry.ProjectName, entry.TaskName).Minutes += entry.Minutes;
                }

                if (!isFormer && status != TimePeriodStatus.Approved && autoFills.TryGetValue(user.Id, out var autoFill))
                {
                    var byDay = entries[user.Id].ToLookup(entry => entry.Date.Date);
                    var filled = calendar.Days.Sum(day => calendar.AutoFillMinutes(
                        day,
                        byDay[day].Where(entry => !entry.IsOvertime).Sum(entry => entry.Minutes),
                        byDay[day].Any(entry => entry.TaskId == autoFill.TaskId)));
                    if (filled > 0)
                    {
                        Row(rows, user.Id, autoFill.ProjectId, autoFill.TaskId, autoFill.CustomerName, autoFill.ProjectName, autoFill.TaskName).Minutes += filled;
                    }
                }

                var worked = rows.Values.Where(row => row.Minutes > 0).ToList();
                projects.AddRange(worked);
                var approved = isFormer
                    ? new List<TimeReportProjectDto>()
                    : calendar.Days
                        .Where(day => calendar.Absences.TryGetValue(day, out var absence) && absence.Status == VacationRequestStatus.Approved)
                        .GroupBy(day => calendar.Absences[day].Type)
                        .Select(group => new TimeReportProjectDto
                        {
                            EmployeeId = user.Id,
                            AbsenceType = group.Key.ToString().ToLowerInvariant(),
                            Minutes = group.Sum(calendar.ExpectedMinutes)
                        })
                        .Where(row => row.Minutes > 0)
                        .ToList();
                absences.AddRange(approved);
                employees.Add(new TimeReportEmployeeDto
                {
                    Id = user.Id,
                    FullName = TimeTableService.FullName(user.FirstName, user.LastName),
                    Email = user.Email,
                    JobTitle = user.JobTitle,
                    PictureId = user.PictureId,
                    ManagerName = TimeTableService.FullName(user.ManagerFirstName, user.ManagerLastName),
                    Status = TimeTableService.Wire(status),
                    LoggedMinutes = worked.Sum(row => row.Minutes) + approved.Sum(row => row.Minutes),
                    ExpectedMinutes = isFormer ? null : calendar.Days.Sum(calendar.ExpectedMinutes),
                    AbsenceMinutes = isFormer ? null : approved.Sum(row => row.Minutes),
                    OvertimeMinutes = entries[user.Id].Where(entry => entry.IsOvertime).Sum(entry => entry.Minutes),
                    IsFormer = isFormer
                });
            }

            var names = employees.ToDictionary(employee => employee.Id, employee => employee.FullName, StringComparer.OrdinalIgnoreCase);
            return new TimeReportDto
            {
                Month = TimeMonth.ToText(first),
                Employees = employees
                    .OrderBy(employee => employee.FullName, StringComparer.CurrentCultureIgnoreCase)
                    .ToList(),
                Projects = projects
                    .OrderBy(row => row.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(row => row.ProjectName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(row => row.TaskName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(row => names[row.EmployeeId], StringComparer.CurrentCultureIgnoreCase)
                    .Concat(absences
                        .OrderBy(row => row.AbsenceType, StringComparer.Ordinal)
                        .ThenBy(row => names[row.EmployeeId], StringComparer.CurrentCultureIgnoreCase))
                    .ToList()
            };
        }

        private static TimeReportProjectDto Row(
            IDictionary<(int ProjectId, int TaskId), TimeReportProjectDto> rows,
            string employeeId,
            int projectId,
            int taskId,
            string customerName,
            string projectName,
            string taskName)
        {
            if (!rows.TryGetValue((projectId, taskId), out var row))
            {
                row = new TimeReportProjectDto
                {
                    EmployeeId = employeeId,
                    CustomerName = customerName,
                    ProjectName = projectName,
                    TaskName = taskName
                };
                rows[(projectId, taskId)] = row;
            }

            return row;
        }

        private class ReportEntry
        {
            public string UserId { get; set; }

            public int ProjectId { get; set; }

            public int TaskId { get; set; }

            public DateTime Date { get; set; }

            public int Minutes { get; set; }

            public string CustomerName { get; set; }

            public string ProjectName { get; set; }

            public string TaskName { get; set; }

            public bool IsOvertime { get; set; }
        }
    }
}
