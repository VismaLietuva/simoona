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
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeTableService : ITimeTableService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly ITimeRegistrationSupport _support;
        private readonly DbSet<TimeProject> _projects;
        private readonly DbSet<TimeTask> _tasks;
        private readonly DbSet<TimeProjectMember> _members;
        private readonly DbSet<TimeEntry> _entries;

        public TimeTableService(IUnitOfWork2 uow, ITimeRegistrationSupport support)
        {
            _uow = uow;
            _support = support;
            _projects = uow.GetDbSet<TimeProject>();
            _tasks = uow.GetDbSet<TimeTask>();
            _members = uow.GetDbSet<TimeProjectMember>();
            _entries = uow.GetDbSet<TimeEntry>();
        }

        public async Task<TimeTableDto> GetTimeTableAsync(string month, string employeeId, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            var last = TimeMonth.LastDay(first);
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            var organizationId = actor.OrganizationId;
            var calendar = await _support.CalendarAsync(actor.UserId, first, organizationId);

            var entries = await _entries
                .AsNoTracking()
                .Where(entry => entry.OrganizationId == organizationId && entry.UserId == actor.UserId && entry.Date >= first && entry.Date <= last)
                .Select(entry => new { entry.ProjectId, entry.TaskId, entry.Date, entry.Minutes })
                .ToListAsync();
            var assigned = (await AssignedProjectIdsAsync(actor)).ToHashSet();
            var withHours = entries.Where(entry => entry.Minutes > 0).Select(entry => entry.ProjectId).ToHashSet();
            var shownIds = assigned.Concat(withHours).Distinct().ToList();

            var projects = await _projects
                .AsNoTracking()
                .Where(project => project.OrganizationId == organizationId && shownIds.Contains(project.Id))
                .Select(project => new
                {
                    project.Id,
                    project.Name,
                    CustomerName = project.Customer.Name,
                    project.IsArchived,
                    Tasks = project.Tasks
                        .Where(task => !task.IsDeleted)
                        .OrderBy(task => task.Id)
                        .Select(task => new TimeTaskDto { Id = task.Id, Name = task.Name, Comment = task.Comment ?? string.Empty, IsDefault = task.IsDefault })
                })
                .ToListAsync();

            var available = await _projects
                .AsNoTracking()
                .Where(project => project.OrganizationId == organizationId && !project.IsArchived && !project.Customer.IsDeleted && !assigned.Contains(project.Id))
                .Select(project => new TimeProjectOptionDto { Id = project.Id, Name = project.Name, CustomerName = project.Customer.Name })
                .ToListAsync();

            var monthLock = await _support.LockAsync(first, organizationId);

            return new TimeTableDto
            {
                Month = TimeMonth.ToText(first),
                Lock = monthLock == null
                    ? null
                    : new TimeMonthLockDto { LockedAt = monthLock.Created, LockedBy = FullName(monthLock.LockedBy?.FirstName, monthLock.LockedBy?.LastName) },
                DailyMinutes = calendar.DailyMinutes,
                Status = Wire(await _support.StatusAsync(actor.UserId, first, organizationId)),
                Days = calendar.Days
                    .Select(day => new TimeTableDayDto
                    {
                        Date = TimeMonth.ToDay(day),
                        ExpectedMinutes = calendar.ExpectedMinutes(day),
                        Holiday = calendar.Holidays.TryGetValue(day, out var holiday) ? holiday : null
                    })
                    .ToList(),
                Projects = projects
                    .Where(project => withHours.Contains(project.Id) || (assigned.Contains(project.Id) && !project.IsArchived))
                    .OrderBy(project => project.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(project => project.Name, StringComparer.CurrentCultureIgnoreCase)
                    .Select(project => new TimeTableProjectDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        CustomerName = project.CustomerName,
                        IsArchived = project.IsArchived,
                        IsAssigned = assigned.Contains(project.Id),
                        Tasks = project.Tasks.ToList()
                    })
                    .ToList(),
                Entries = entries
                    .Select(entry => new TimeEntryDto { ProjectId = entry.ProjectId, TaskId = entry.TaskId, Date = TimeMonth.ToDay(entry.Date), Minutes = entry.Minutes })
                    .ToList(),
                Absences = calendar.Absences.Values
                    .OrderBy(absence => absence.Date)
                    .Select(absence => new TimeAbsenceDto
                    {
                        Date = TimeMonth.ToDay(absence.Date),
                        Type = absence.Type.ToString().ToLowerInvariant(),
                        Status = absence.Status.ToString().ToLowerInvariant()
                    })
                    .ToList(),
                AutoFillProjectId = await _support.AutoFillProjectIdAsync(actor.UserId, organizationId),
                AvailableProjects = available
                    .OrderBy(project => project.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(project => project.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList()
            };
        }

        public async Task SaveEntriesAsync(string month, string employeeId, IList<TimeEntryChangeDto> changes, UserAndOrganizationDto userOrg)
        {
            var first = TimeMonth.Parse(month);
            var last = TimeMonth.LastDay(first);
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            var organizationId = actor.OrganizationId;
            await ThrowIfNotEditableAsync(actor, first);

            var parsed = (changes ?? new List<TimeEntryChangeDto>())
                .Select(change => new { change.ProjectId, change.TaskId, Date = TimeMonth.ParseDay(change.Date), change.Minutes })
                .GroupBy(change => (change.TaskId, change.Date))
                .Select(group => group.Last())
                .ToList();
            if (parsed.Count == 0)
            {
                return;
            }

            var projectIds = parsed.Select(change => change.ProjectId).Distinct().ToList();
            var taskIds = parsed.Select(change => change.TaskId).Distinct().ToList();
            var projects = await _projects
                .AsNoTracking()
                .Where(project => project.OrganizationId == organizationId && projectIds.Contains(project.Id))
                .Select(project => new { project.Id, project.Name, project.IsArchived })
                .ToListAsync();
            var tasks = await _tasks
                .AsNoTracking()
                .Where(task => task.OrganizationId == organizationId && taskIds.Contains(task.Id))
                .Select(task => new { task.Id, task.ProjectId })
                .ToListAsync();
            var assigned = (await AssignedProjectIdsAsync(actor)).ToHashSet();

            foreach (var change in parsed)
            {
                if (change.Date < first || change.Date > last)
                {
                    throw Invalid("Date is outside this month");
                }

                if (change.Minutes.HasValue && (change.Minutes < 0 || change.Minutes > TimeRegistrationRules.MaxDayMinutes))
                {
                    throw Invalid("Hours must be between 0 and 24");
                }

                var project = projects.FirstOrDefault(row => row.Id == change.ProjectId)
                              ?? throw new ValidationException(ErrorCodes.TimeRegistrationNotFound, "Project not found");
                if (project.IsArchived)
                {
                    throw Invalid($"Project \"{project.Name}\" is archived");
                }

                if (!assigned.Contains(project.Id))
                {
                    throw Invalid($"Project \"{project.Name}\" is not assigned");
                }

                if (!tasks.Any(task => task.Id == change.TaskId && task.ProjectId == project.Id))
                {
                    throw new ValidationException(ErrorCodes.TimeRegistrationNotFound, "Task not found");
                }
            }

            var dates = parsed.Select(change => change.Date).Distinct().ToList();
            var existing = await _entries
                .Where(entry => entry.OrganizationId == organizationId && entry.UserId == actor.UserId && dates.Contains(entry.Date))
                .ToListAsync();

            var totals = dates.ToDictionary(
                date => date,
                date => existing.Where(entry => entry.Date.Date == date).ToDictionary(entry => entry.TaskId, entry => entry.Minutes));
            foreach (var change in parsed)
            {
                if (change.Minutes.HasValue)
                {
                    totals[change.Date][change.TaskId] = change.Minutes.Value;
                }
                else
                {
                    totals[change.Date].Remove(change.TaskId);
                }
            }

            var overLimit = totals.FirstOrDefault(day => day.Value.Values.Sum() > TimeRegistrationRules.MaxDayMinutes);
            if (overLimit.Value != null)
            {
                throw Invalid($"More than 24 hours on {TimeMonth.ToDay(overLimit.Key)}");
            }

            foreach (var change in parsed)
            {
                var row = existing.FirstOrDefault(entry => entry.TaskId == change.TaskId && entry.Date.Date == change.Date);
                if (!change.Minutes.HasValue)
                {
                    if (row != null)
                    {
                        _entries.Remove(row);
                    }

                    continue;
                }

                if (row != null)
                {
                    row.Minutes = change.Minutes.Value;
                    row.ProjectId = change.ProjectId;
                    continue;
                }

                _entries.Add(new TimeEntry
                {
                    OrganizationId = organizationId,
                    UserId = actor.UserId,
                    ProjectId = change.ProjectId,
                    TaskId = change.TaskId,
                    Date = change.Date,
                    Minutes = change.Minutes.Value
                });
            }

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task SetAutoFillAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            var members = await _members
                .Include(member => member.Project)
                .Where(member => member.OrganizationId == actor.OrganizationId && member.UserId == actor.UserId)
                .ToListAsync();
            var chosen = members.FirstOrDefault(member => member.ProjectId == projectId && member.Project != null && !member.Project.IsArchived && !member.Project.IsDeleted)
                         ?? throw Invalid("Project is not assigned");

            foreach (var member in members)
            {
                member.IsAutoFill = member == chosen;
            }

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task AddProjectAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            var project = await _projects
                .AsNoTracking()
                .Where(row => row.OrganizationId == actor.OrganizationId && row.Id == projectId)
                .Select(row => new { row.Id, row.Name, row.IsArchived })
                .FirstOrDefaultAsync() ?? throw new ValidationException(ErrorCodes.TimeRegistrationNotFound, "Project not found");
            if (project.IsArchived)
            {
                throw Invalid($"Project \"{project.Name}\" is archived");
            }

            if (await _members.AnyAsync(member => member.ProjectId == projectId && member.UserId == actor.UserId))
            {
                return;
            }

            _members.Add(new TimeProjectMember { OrganizationId = actor.OrganizationId, ProjectId = projectId, UserId = actor.UserId });
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task RemoveProjectAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg)
        {
            var actor = await _support.ResolveAsync(userOrg, employeeId);
            var member = await _members
                .FirstOrDefaultAsync(row => row.OrganizationId == actor.OrganizationId && row.ProjectId == projectId && row.UserId == actor.UserId);
            if (member == null)
            {
                return;
            }

            _members.Remove(member);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        internal static string Wire(TimePeriodStatus status)
        {
            return status.ToString().ToLowerInvariant();
        }

        internal static string FullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }

        private async Task ThrowIfNotEditableAsync(TimeActor actor, DateTime month)
        {
            await _support.ThrowIfLockedAsync(month, actor.OrganizationId);
            var status = await _support.StatusAsync(actor.UserId, month, actor.OrganizationId);

            if (!actor.IsSelf && status == TimePeriodStatus.Approved)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationNotEditable, "Unapprove this month to make changes");
            }

            if (actor.IsSelf && status != TimePeriodStatus.Open)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationNotEditable, "This month is already submitted");
            }
        }

        private async Task<List<int>> AssignedProjectIdsAsync(TimeActor actor)
        {
            return await _members
                .AsNoTracking()
                .Where(member => member.OrganizationId == actor.OrganizationId && member.UserId == actor.UserId)
                .Select(member => member.ProjectId)
                .ToListAsync();
        }

        private static ValidationException Invalid(string message)
        {
            return new ValidationException(ErrorCodes.TimeRegistrationInvalid, message);
        }
    }
}
