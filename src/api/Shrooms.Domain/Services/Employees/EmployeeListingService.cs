using Microsoft.EntityFrameworkCore;
using X.PagedList;
﻿using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Extensions;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Employees
{
    public class EmployeeListingService : IEmployeeListingService
    {
        private const string BirthDaySortProperty = "BirthDay";

        private const char SortablePropertiesSeparator = ';';
        private const char SortablePropertySeparator = ' ';

        private readonly DbSet<ApplicationUser> _usersDbSet;

        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;

        public EmployeeListingService(
            IUnitOfWork2 uow,
            IPermissionService permissionService,
            IRoleService roleService,
            ISystemClock systemClock)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();

            _permissionService = permissionService;
            _roleService = roleService;
            _systemClock = systemClock;
        }

        public async Task<IPagedList<EmployeeDto>> GetPagedEmployeesAsync(EmployeeListingArgsDto employeeArgsDto, UserAndOrganizationDto userOrg)
        {
            var permissions = (await _permissionService.GetUserPermissionsAsync(userOrg.UserId, userOrg.OrganizationId)).ToList();

            var hasApplicationUserPermission = permissions.Contains(AdministrationPermissions.ApplicationUser);
            var hasBlacklistPermission = permissions.Contains(BasicPermissions.Blacklist);

            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Contracts.Constants.Roles.NewUser);

            var searchFilter = GetSearchStringFilter(employeeArgsDto);
            var blacklistFilter = GetBlacklistFilter(employeeArgsDto, hasBlacklistPermission);

            var employeesQuery = _usersDbSet
                .Include(user => user.WorkingHours)
                .Include(user => user.JobPosition)
                .Include(user => user.BlacklistEntries)
                .Where(searchFilter)
                .Where(blacklistFilter)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId))
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .Select(user => new EmployeeDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JobTitle = user.JobPosition.Title,
                    PictureId = user.PictureId,
                    BirthDay = user.BirthDay,
                    PhoneNumber = user.PhoneNumber,
                    WorkingHours = new WorkingHourslWithOutLunchDto
                    {
                        StartTime = user.WorkingHours.StartTime,
                        EndTime = user.WorkingHours.EndTime
                    },
                    BlacklistEntry = user.BlacklistEntries
                        .Where(blacklistUser => blacklistUser.Status == BlacklistStatus.Active)
                        .Select(blacklistUser => new BlacklistUserDto
                        {
                            EndDate = blacklistUser.EndDate
                        })
                        .FirstOrDefault()
                });

            employeesQuery = ApplyOrdering(employeesQuery, employeeArgsDto);

            // X.PagedList doesn't have async support for IQueryable, need to materialize first
            var totalCount = await employeesQuery.CountAsync();
            var items = await employeesQuery
                .Skip((employeeArgsDto.Page - 1) * employeeArgsDto.PageSize)
                .Take(employeeArgsDto.PageSize)
                .ToListAsync();

            var users = new StaticPagedList<EmployeeDto>(items, employeeArgsDto.Page, employeeArgsDto.PageSize, totalCount);

            HidePrivateInformationBasedOnPermissions(users, hasApplicationUserPermission, hasBlacklistPermission);

            return users;
        }

        // The list only ever shows a birthday's month and day, so ordering by the
        // stored date would order by age and look random. Order by whose birthday
        // falls next instead, wrapping at the end of the year.
        private IQueryable<EmployeeDto> ApplyOrdering(IQueryable<EmployeeDto> employeesQuery, EmployeeListingArgsDto employeeArgsDto)
        {
            if (!TryGetBirthDaySortDirection(employeeArgsDto?.SortByProperties, out var isDescending))
            {
                return employeesQuery.OrderByPropertyNames(employeeArgsDto);
            }

            var today = _systemClock.UtcNow.Date;
            var todayMonthAndDay = (today.Month * 100) + today.Day;

            // 0 = still to come this year, 1 = already passed.
            Expression<Func<EmployeeDto, int>> hasPassed = employee =>
                employee.BirthDay.HasValue &&
                ((employee.BirthDay.Value.Month * 100) + employee.BirthDay.Value.Day) < todayMonthAndDay
                    ? 1
                    : 0;

            Expression<Func<EmployeeDto, int>> byMonthAndDay = employee => employee.BirthDay.HasValue
                ? (employee.BirthDay.Value.Month * 100) + employee.BirthDay.Value.Day
                : 0;

            // Employees with no birthday stay last either way — there is nothing
            // to place them among the dates.
            var ordered = employeesQuery.OrderBy(employee => employee.BirthDay.HasValue ? 0 : 1);

            ordered = isDescending
                ? ordered.ThenByDescending(hasPassed).ThenByDescending(byMonthAndDay)
                : ordered.ThenBy(hasPassed).ThenBy(byMonthAndDay);

            // Id last, so paging can't repeat or skip employees sharing a birthday.
            return ordered
                .ThenBy(employee => employee.LastName)
                .ThenBy(employee => employee.FirstName)
                .ThenBy(employee => employee.Id);
        }

        private static bool TryGetBirthDaySortDirection(string sortByProperties, out bool isDescending)
        {
            isDescending = false;

            var firstProperty = sortByProperties?
                .Split(SortablePropertiesSeparator, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (firstProperty == null)
            {
                return false;
            }

            var propertyParts = firstProperty.Split(SortablePropertySeparator, StringSplitOptions.RemoveEmptyEntries);

            if (!string.Equals(propertyParts.FirstOrDefault(), BirthDaySortProperty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            isDescending = string.Equals(
                propertyParts.LastOrDefault(),
                SortDirectionConstants.Descending,
                StringComparison.OrdinalIgnoreCase);

            return true;
        }

        private void HidePrivateInformationBasedOnPermissions(IPagedList<EmployeeDto> employees, bool hasApplicationUserPermission, bool hasBlacklistPermission)
        {
            if (hasApplicationUserPermission && hasBlacklistPermission)
            {
                return;
            }

            foreach (var employee in employees)
            {
                if (!hasApplicationUserPermission)
                {
                    employee.BirthDay = BirthdayDateTimeHelper.RemoveYear(employee.BirthDay);
                    employee.PhoneNumber = null;
                }

                if (!hasBlacklistPermission)
                {
                    employee.BlacklistEntry = null;
                }
            }
        }

        private Expression<Func<ApplicationUser, bool>> GetBlacklistFilter(EmployeeListingArgsDto employeeArgsDto, bool hasBlacklistPermission)
        {
            if (!employeeArgsDto.ShowOnlyBlacklisted || !hasBlacklistPermission)
            {
                return user => true;
            }

            return user => user.BlacklistEntries.Any(blacklistUser => blacklistUser.Status == BlacklistStatus.Active);
        }

        private static Expression<Func<ApplicationUser, bool>> GetSearchStringFilter(EmployeeListingArgsDto employeeArgsDto)
        {
            if (employeeArgsDto.Search == null)
            {
                return user => true;
            }

            var searchWords = employeeArgsDto.Search.Split(WebApiConstants.SearchSplitter);

            return user => searchWords
                .Count(sw => user.FirstName.Contains(sw) ||
                             user.LastName.Contains(sw) ||
                             user.JobPosition.Title.Contains(sw)) == searchWords.Count();
        }
    }
}
