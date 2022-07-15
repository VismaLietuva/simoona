using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Employees;
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
            var isAdmin = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.ApplicationUser);
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Contracts.Constants.Roles.NewUser);

            var filter = GetSearchStringFilter(employeeArgsDto.Search);

            var users = await _usersDbSet
                .Include(user => user.WorkingHours)
                .Include(user => user.JobPosition)
                .Where(filter)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId))
                .OrderByPropertyNames(employeeArgsDto)
                .ToPagedListAsync(employeeArgsDto.Page, employeeArgsDto.PageSize);

            return users.Select(user => new EmployeeDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JobTitle = user.JobPosition?.Title,
                BirthDay = isAdmin ? user.BirthDay : BirthdayDateTimeHelper.RemoveYear(user.BirthDay),
                PhoneNumber = isAdmin ? user.PhoneNumber : null,
                WorkingHours = new WorkingHourslWithOutLunchDto
                {
                    StartTime = user.WorkingHours?.StartTime,
                    EndTime = user.WorkingHours?.EndTime
                }
            });
        }

        private static Expression<Func<ApplicationUser, bool>> GetSearchStringFilter(string searchString)
        {
            if (searchString == null)
            {
                return user => true;
            }

            var searchWords = searchString.Split(WebApiConstants.SearchSplitter);

            return user => searchWords
                .Count(sw => user.FirstName.Contains(sw) ||
                             user.LastName.Contains(sw) || 
                             user.JobPosition.Title.Contains(sw)) == searchWords.Count();
        }
    }
}
