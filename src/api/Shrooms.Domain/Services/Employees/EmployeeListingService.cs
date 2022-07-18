using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Extensions;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Domain.Services.Employees
{
    public class EmployeeListingService : IEmployeeListingService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

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
            var isAdmin = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.ApplicationUser);
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Contracts.Constants.Roles.NewUser);

            var searchFilter = GetSearchStringFilter(employeeArgsDto);
            var blacklistFilter = GetBlacklistFilter(employeeArgsDto, isAdmin);

            var users = await _usersDbSet
                .Include(user => user.WorkingHours)
                .Include(user => user.JobPosition)
                .Include(user => user.BlacklistStates)
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
                    BirthDay = user.BirthDay,
                    PhoneNumber = user.PhoneNumber,
                    WorkingHours = new WorkingHourslWithOutLunchDto
                    {
                        StartTime = user.WorkingHours.StartTime,
                        EndTime = user.WorkingHours.EndTime
                    },
                    BlacklistState = user.BlacklistStates
                        .Where(blacklistState => blacklistState.EndDate > _systemClock.UtcNow)
                        .Select(blacklistState => new BlacklistStateDto
                        {
                            EndDate = blacklistState.EndDate,
                        })
                        .FirstOrDefault()
                })
                .OrderByPropertyNames(employeeArgsDto)
                .ToPagedListAsync(employeeArgsDto.Page, employeeArgsDto.PageSize);
            
            if (!isAdmin)
            {
                HidePrivateInformation(users);
            }

            return users;
        }

        private void HidePrivateInformation(IPagedList<EmployeeDto> employees)
        {
            foreach (var employee in employees)
            {
                employee.BirthDay = BirthdayDateTimeHelper.RemoveYear(employee.BirthDay);
                employee.PhoneNumber = null;
                employee.BlacklistState = null;
            }
        }

        private Expression<Func<ApplicationUser, bool>> GetBlacklistFilter(EmployeeListingArgsDto employeeArgsDto, bool isAdmin)
        {
            if (!employeeArgsDto.ShowOnlyBlacklisted || !isAdmin)
            {
                return user => true;
            }

            return user => user.BlacklistStates.Any(blacklistState => blacklistState.EndDate > _systemClock.UtcNow);
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
