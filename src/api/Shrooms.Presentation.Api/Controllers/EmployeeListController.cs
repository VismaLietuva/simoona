using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using WebApi.OutputCache.V2;

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
        [CacheOutput(ServerTimeSpan = WebApiConstants.OneDay)]
        public PagedViewModel<EmployeeListViewModel> GetPaged(int page = 1, string filter = "", string search = "", string sortBy = "LastName", string sortOrder = "asc")
        {
            if (!string.IsNullOrEmpty(search))
            {
                var searchWords = search.Split(WebApiConstants.SearchSplitter);
                return GetFilteredPaged("WorkingHours,JobPosition", page, WebApiConstants.DefaultPageSize, sortBy, sortOrder, s => searchWords.Count(sw => s.FirstName.Contains(sw) || s.LastName.Contains(sw) || s.JobPosition.Title.Contains(sw)) == searchWords.Count());
            }

            return GetFilteredPaged("WorkingHours,JobPosition", page, WebApiConstants.DefaultPageSize, sortBy, sortOrder);
        }

        protected virtual PagedViewModel<EmployeeListViewModel> GetFilteredPaged(
            string includeProperties = null, int page = 1, int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<ApplicationUser, bool>> filter = null)
        {
            var isAdmin = _permissionService.UserHasPermission(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser);

            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var models = _applicationUserRepository.Get(
                includeProperties: includeProperties, filter: filter, orderBy: sortQuery)
                .Where(_roleService.ExcludeUsersWithRole(Roles.NewUser))
                .ToPagedList(page, pageSize);

            var pagedModel = new StaticPagedList<EmployeeListViewModel>(_mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<EmployeeListViewModel>>(models), models.PageNumber, models.PageSize, models.TotalItemCount);

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
