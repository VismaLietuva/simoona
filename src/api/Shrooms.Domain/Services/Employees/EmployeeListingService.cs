using LinqKit;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
﻿using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Extensions;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Employees
{
    public class EmployeeListingService : IEmployeeListingService
    {
        private static readonly HashSet<string> SortableProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(EmployeeDto.Id),
            nameof(EmployeeDto.FirstName),
            nameof(EmployeeDto.LastName),
            nameof(EmployeeDto.JobTitle),
            nameof(EmployeeDto.BirthDay)
        };

        private static readonly string BlacklistSortProperty = $"{nameof(EmployeeDto.BlacklistEntry)}.{nameof(BlacklistUserDto.EndDate)}";

        private readonly DbSet<ApplicationUser> _usersDbSet;

        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;

        public EmployeeListingService(
            IUnitOfWork2 uow,
            IPermissionService permissionService,
            IRoleService roleService)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();

            _permissionService = permissionService;
            _roleService = roleService;
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
                .Select(GetEmployeeProjection(hasApplicationUserPermission, hasBlacklistPermission))
                .OrderByPropertyNames(GetSafeSortable(employeeArgsDto, hasBlacklistPermission));

            // X.PagedList doesn't have async support for IQueryable, need to materialize first
            var totalCount = await employeesQuery.CountAsync();
            var items = await employeesQuery
                .Skip((employeeArgsDto.Page - 1) * employeeArgsDto.PageSize)
                .Take(employeeArgsDto.PageSize)
                .ToListAsync();

            return new StaticPagedList<EmployeeDto>(items, employeeArgsDto.Page, employeeArgsDto.PageSize, totalCount);
        }

        // Private fields are hidden inside the SQL projection, so sorting and paging never see the real values.
        private static Expression<Func<ApplicationUser, EmployeeDto>> GetEmployeeProjection(bool hasApplicationUserPermission, bool hasBlacklistPermission)
        {
            Expression<Func<ApplicationUser, DateTime?>> birthDay = hasApplicationUserPermission
                ? user => user.BirthDay
                : user => user.BirthDay.HasValue
                    ? user.BirthDay.Value.Date.AddYears(BirthdayDateTimeHelper.HiddenYear - user.BirthDay.Value.Year)
                    : null;

            Expression<Func<ApplicationUser, string>> phoneNumber = hasApplicationUserPermission
                ? user => user.PhoneNumber
                : user => null;

            Expression<Func<ApplicationUser, BlacklistUserDto>> blacklistEntry = hasBlacklistPermission
                ? user => user.BlacklistEntries
                    .Where(blacklistUser => blacklistUser.Status == BlacklistStatus.Active)
                    .Select(blacklistUser => new BlacklistUserDto
                    {
                        EndDate = blacklistUser.EndDate
                    })
                    .FirstOrDefault()
                : user => null;

            Expression<Func<ApplicationUser, EmployeeDto>> projection = user => new EmployeeDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JobTitle = user.JobPosition.Title,
                PictureId = user.PictureId,
                BirthDay = birthDay.Invoke(user),
                PhoneNumber = phoneNumber.Invoke(user),
                WorkingHours = new WorkingHourslWithOutLunchDto
                {
                    StartTime = user.WorkingHours.StartTime,
                    EndTime = user.WorkingHours.EndTime
                },
                BlacklistEntry = blacklistEntry.Invoke(user)
            };

            return projection.Expand();
        }

        private static EmployeeListingArgsDto GetSafeSortable(EmployeeListingArgsDto employeeArgsDto, bool hasBlacklistPermission)
        {
            var requestedSorts = (employeeArgsDto.SortByProperties ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(sort => IsSortable(sort.Split(' ')[0], hasBlacklistPermission))
                .ToList();

            // Stable tie-breaker so equal sort keys never fall back to an order that could reveal hidden data.
            if (!requestedSorts.Any(sort => sort.Split(' ')[0].Equals(nameof(EmployeeDto.Id), StringComparison.OrdinalIgnoreCase)))
            {
                requestedSorts.Add($"{nameof(EmployeeDto.Id)} {SortDirectionConstants.Ascending}");
            }

            return new EmployeeListingArgsDto
            {
                SortByProperties = string.Join(";", requestedSorts)
            };
        }

        private static bool IsSortable(string propertyName, bool hasBlacklistPermission)
        {
            return SortableProperties.Contains(propertyName) ||
                   (hasBlacklistPermission && BlacklistSortProperty.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
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
