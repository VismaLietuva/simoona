using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeAccountingService : ITimeAccountingService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<TimeCustomer> _customers;
        private readonly DbSet<TimeProject> _projects;
        private readonly DbSet<TimeTask> _tasks;
        private readonly DbSet<TimeProjectMember> _members;
        private readonly DbSet<TimeEntry> _entries;
        private readonly DbSet<ApplicationUser> _users;

        public TimeAccountingService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _customers = uow.GetDbSet<TimeCustomer>();
            _projects = uow.GetDbSet<TimeProject>();
            _tasks = uow.GetDbSet<TimeTask>();
            _members = uow.GetDbSet<TimeProjectMember>();
            _entries = uow.GetDbSet<TimeEntry>();
            _users = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<IEnumerable<TimeCustomerDto>> GetCustomersAsync(UserAndOrganizationDto userOrg)
        {
            var customers = await _customers
                .AsNoTracking()
                .Where(customer => customer.OrganizationId == userOrg.OrganizationId)
                .Select(customer => new
                {
                    customer.Id,
                    customer.Name,
                    ProjectCount = customer.Projects.Count(project => !project.IsDeleted),
                    Members = customer.Projects
                        .Where(project => !project.IsDeleted)
                        .SelectMany(project => project.Members.Where(member => !member.User.IsDeleted).Select(member => member.UserId))
                })
                .ToListAsync();

            return customers
                .OrderBy(customer => customer.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(customer => new TimeCustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    ProjectCount = customer.ProjectCount,
                    MemberCount = customer.Members.Distinct().Count()
                })
                .ToList();
        }

        public async Task<int> SaveCustomerAsync(TimeCustomerInputDto input, UserAndOrganizationDto userOrg)
        {
            var name = RequiredName(input.Name, TimeCustomer.MaxNameLength, "Customer name is required");
            var siblings = await _customers
                .Where(customer => customer.OrganizationId == userOrg.OrganizationId)
                .ToListAsync();

            if (siblings.Any(customer => customer.Id != input.Id && TimeRegistrationRules.SameName(customer.Name, name)))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationDuplicate, $"Customer \"{name}\" already exists");
            }

            if (input.Id.HasValue)
            {
                var existing = siblings.FirstOrDefault(customer => customer.Id == input.Id) ?? throw NotFound("Customer");
                existing.Name = name;
                await SaveNamesAsync(userOrg.UserId);
                return existing.Id;
            }

            var created = new TimeCustomer { OrganizationId = userOrg.OrganizationId, Name = name };
            _customers.Add(created);
            await SaveNamesAsync(userOrg.UserId);
            return created.Id;
        }

        public async Task DeleteCustomerAsync(int id, UserAndOrganizationDto userOrg)
        {
            var customer = await _customers
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == id) ?? throw NotFound("Customer");

            if (await _projects.AnyAsync(project => project.CustomerId == id && !project.IsDeleted))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInUse, "Customer still has projects");
            }

            customer.IsDeleted = true;
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<IEnumerable<TimeProjectDto>> GetProjectsAsync(UserAndOrganizationDto userOrg)
        {
            var projects = await _projects
                .AsNoTracking()
                .Where(project => project.OrganizationId == userOrg.OrganizationId && !project.Customer.IsDeleted)
                .Select(project => new
                {
                    project.Id,
                    project.Name,
                    project.CustomerId,
                    CustomerName = project.Customer.Name,
                    project.IsArchived,
                    Tasks = project.Tasks
                        .Where(task => !task.IsDeleted)
                        .OrderBy(task => task.Id)
                        .Select(task => new TimeTaskDto { Id = task.Id, Name = task.Name, Comment = task.Comment ?? string.Empty, IsDefault = task.IsDefault }),
                    Members = project.Members.Select(member => new TimeEmployeeDto
                    {
                        Id = member.UserId,
                        FullName = ((member.User.FirstName ?? string.Empty) + " " + (member.User.LastName ?? string.Empty)).Trim(),
                        Email = member.User.Email,
                        JobTitle = member.User.JobPosition != null ? member.User.JobPosition.Title : null,
                        PictureId = member.User.PictureId
                    })
                })
                .ToListAsync();

            return projects
                .OrderBy(project => project.CustomerName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(project => project.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(project => new TimeProjectDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    CustomerId = project.CustomerId,
                    CustomerName = project.CustomerName,
                    IsArchived = project.IsArchived,
                    Tasks = project.Tasks.ToList(),
                    Members = project.Members.OrderBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase).ToList()
                })
                .ToList();
        }

        public async Task<int> SaveProjectAsync(TimeProjectInputDto input, UserAndOrganizationDto userOrg)
        {
            var name = RequiredName(input.Name, TimeProject.MaxNameLength, "Project name is required");
            var tasks = input.Tasks ?? new List<TimeTaskInputDto>();
            ThrowIfInvalidTasks(tasks);

            if (!await _customers.AnyAsync(customer => customer.OrganizationId == userOrg.OrganizationId && customer.Id == input.CustomerId))
            {
                throw NotFound("Customer");
            }

            var siblings = await _projects
                .Where(project => project.OrganizationId == userOrg.OrganizationId && project.CustomerId == input.CustomerId)
                .Select(project => new { project.Id, project.Name })
                .ToListAsync();
            if (siblings.Any(project => project.Id != input.Id && TimeRegistrationRules.SameName(project.Name, name)))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationDuplicate, $"Project \"{name}\" already exists for this customer");
            }

            var memberIds = (input.MemberIds ?? new List<string>()).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var knownMembers = await _users
                .Where(user => user.OrganizationId == userOrg.OrganizationId && memberIds.Contains(user.Id))
                .Select(user => user.Id)
                .ToListAsync();
            if (knownMembers.Count != memberIds.Count)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationNotFound, "Employee not found");
            }

            memberIds = knownMembers;

            TimeProject project;
            if (input.Id.HasValue)
            {
                project = await _projects
                    .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == input.Id) ?? throw NotFound("Project");
                project.Name = name;
                project.CustomerId = input.CustomerId;
            }
            else
            {
                project = new TimeProject { OrganizationId = userOrg.OrganizationId, CustomerId = input.CustomerId, Name = name };
                _projects.Add(project);
            }

            var existingTasks = project.Id == 0
                ? new List<TimeTask>()
                : await _tasks.Where(task => task.ProjectId == project.Id).ToListAsync();
            var keptIds = tasks.Where(task => task.Id.HasValue).Select(task => task.Id.Value).ToHashSet();
            var removed = existingTasks.Where(task => !keptIds.Contains(task.Id)).ToList();
            await ThrowIfTasksHaveHoursAsync(removed);

            foreach (var task in removed)
            {
                task.IsDeleted = true;
            }

            foreach (var task in tasks)
            {
                var row = task.Id.HasValue ? existingTasks.FirstOrDefault(existing => existing.Id == task.Id) : null;
                if (task.Id.HasValue && row == null)
                {
                    throw NotFound("Task");
                }

                if (row == null)
                {
                    row = new TimeTask { OrganizationId = userOrg.OrganizationId, Project = project };
                    _tasks.Add(row);
                }

                row.Name = task.Name.Trim();
                row.Comment = task.Comment?.Trim() ?? string.Empty;
                row.IsDefault = task.IsDefault;
            }

            var existingMembers = project.Id == 0
                ? new List<TimeProjectMember>()
                : await _members.Where(member => member.ProjectId == project.Id).ToListAsync();
            foreach (var member in existingMembers.Where(member => !memberIds.Contains(member.UserId, StringComparer.OrdinalIgnoreCase)))
            {
                _members.Remove(member);
            }

            foreach (var userId in memberIds.Where(id => existingMembers.All(member => !TimeRegistrationRules.SameUser(member.UserId, id))))
            {
                _members.Add(new TimeProjectMember { OrganizationId = userOrg.OrganizationId, Project = project, UserId = userId });
            }

            await SaveNamesAsync(userOrg.UserId);
            return project.Id;
        }

        public async Task SetProjectArchivedAsync(int id, bool isArchived, UserAndOrganizationDto userOrg)
        {
            var project = await _projects
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == id) ?? throw NotFound("Project");

            project.IsArchived = isArchived;
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task DeleteProjectAsync(int id, UserAndOrganizationDto userOrg)
        {
            var project = await _projects
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == id) ?? throw NotFound("Project");

            if (await _entries.AnyAsync(entry => entry.ProjectId == id))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInUse, $"Project \"{project.Name}\" has registered hours. Archive it instead");
            }

            foreach (var task in await _tasks.Where(task => task.ProjectId == id).ToListAsync())
            {
                task.IsDeleted = true;
            }

            foreach (var member in await _members.Where(member => member.ProjectId == id).ToListAsync())
            {
                _members.Remove(member);
            }

            project.IsDeleted = true;
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private async Task ThrowIfTasksHaveHoursAsync(IReadOnlyCollection<TimeTask> tasks)
        {
            var ids = tasks.Select(task => task.Id).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            var used = await _entries
                .Where(entry => ids.Contains(entry.TaskId))
                .Select(entry => entry.TaskId)
                .FirstOrDefaultAsync();
            if (used != 0)
            {
                var task = tasks.First(row => row.Id == used);
                throw new ValidationException(ErrorCodes.TimeRegistrationInUse, $"Task \"{task.Name}\" has registered hours and can't be removed");
            }
        }

        private static void ThrowIfInvalidTasks(IList<TimeTaskInputDto> tasks)
        {
            if (tasks.Count == 0)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, "A project needs at least one task");
            }

            foreach (var task in tasks)
            {
                RequiredName(task.Name, TimeTask.MaxNameLength, "Task name is required");
                ThrowIfCommentTooLong(task.Comment);
            }

            if (tasks.Where(task => task.Id.HasValue).GroupBy(task => task.Id).Any(group => group.Count() > 1))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationDuplicate, "The same task is listed twice");
            }

            if (tasks.Count(task => task.IsDefault) != 1)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, "Exactly one task must be the default");
            }

            for (var i = 0; i < tasks.Count; i++)
            {
                if (tasks.Take(i).Any(other => TimeRegistrationRules.SameName(other.Name, tasks[i].Name)))
                {
                    throw new ValidationException(ErrorCodes.TimeRegistrationDuplicate, $"Task \"{tasks[i].Name.Trim()}\" is listed twice");
                }
            }
        }

        private async Task SaveNamesAsync(string userId)
        {
            try
            {
                await _uow.SaveChangesAsync(userId);
            }
            catch (DbUpdateException e) when (e.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationDuplicate, "A name is already taken. Save one rename at a time");
            }
        }

        private static void ThrowIfCommentTooLong(string comment)
        {
            if (comment?.Trim().Length > TimeTask.MaxCommentLength)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, $"Comment can't be longer than {TimeTask.MaxCommentLength} characters");
            }
        }

        private static string RequiredName(string value, int maxLength, string message)
        {
            var name = value?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, message);
            }

            if (name.Length > maxLength)
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, $"Name can't be longer than {maxLength} characters");
            }

            return name;
        }

        private static ValidationException NotFound(string entity)
        {
            return new ValidationException(ErrorCodes.TimeRegistrationNotFound, $"{entity} not found");
        }
    }
}
