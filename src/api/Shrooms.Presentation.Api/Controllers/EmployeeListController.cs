using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class EmployeeListController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRoleService _roleService;

        public EmployeeListController(IMapper mapper, IUnitOfWork unitOfWork, IPermissionService permissionService, IRoleService roleService)
        {
            _mapper = mapper;
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _permissionService = permissionService;
            _roleService = roleService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.EmployeeList)]
        public async Task<PagedViewModel<EmployeeListViewModel>> GetPaged(int page = 1, string filter = "", string search = "", string sortBy = "LastName", string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(search))
            {
                var searchWords = search.Split(WebApiConstants.SearchSplitter);
                return await GetFilteredPaged("WorkingHours,JobPosition", page, WebApiConstants.DefaultPageSize, sortBy, sortOrder,
                    s => searchWords.Count(sw => s.FirstName.Contains(sw) || s.LastName.Contains(sw) || s.JobPosition.Title.Contains(sw)) == searchWords.Length);
            }

            return await GetFilteredPaged("WorkingHours,JobPosition", page, WebApiConstants.DefaultPageSize, sortBy, sortOrder);
        }

        protected async Task<PagedViewModel<EmployeeListViewModel>> GetFilteredPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            Expression<Func<ApplicationUser, bool>> filter = null)
        {
            var isAdmin = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser);

            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var models = await _applicationUserRepository.Get(includeProperties: includeProperties, filter: filter, orderBy: sortQuery)
                .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
                .ToPagedListAsync(page, pageSize);

            var employeeListViewModels = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<EmployeeListViewModel>>(models);
            var pagedModel = new StaticPagedList<EmployeeListViewModel>(employeeListViewModels, models.PageNumber, models.PageSize,
                models.TotalItemCount);

            if (!isAdmin)
            {
                foreach (var model in pagedModel)
                {
                    model.BirthDay = BirthdayDateTimeHelper.RemoveYear(model.BirthDay);
                    model.PhoneNumber = null;
                }
            }

            var result = new PagedViewModel<EmployeeListViewModel>
            {
                PagedList = pagedModel,
                PageCount = pagedModel.PageCount,
                ItemCount = pagedModel.TotalItemCount,
                PageSize = pageSize
            };

            return result;
        }
    }
}
