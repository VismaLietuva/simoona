using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Roles;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Role")]
    public class RolesController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ApplicationRole> _roleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;

        private readonly IRoleService _roleService;

        private readonly ShroomsRoleManager _roleManager;
        private readonly ShroomsUserManager _userManager;

        public RolesController(IMapper mapper,
            IUnitOfWork unitOfWork,
            IPermissionService permissionService,
            ICustomCache<string, IEnumerable<string>> permissionsCache,
            IRoleService roleService,
            ShroomsUserManager userManager = null,
            ShroomsRoleManager roleManager = null)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _permissionsCache = permissionsCache;
            _permissionService = permissionService;
            _roleService = roleService;
            _roleRepository = unitOfWork.GetRepository<ApplicationRole>();
            _permissionRepository = unitOfWork.GetRepository<Permission>();
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();

            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("GetRolesForAutocomplete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<IHttpActionResult> GetRolesForAutoComplete(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return BadRequest("Search string can't be empty");
            }

            var rolesDto = await _roleService.GetRolesForAutocompleteAsync(search, GetUserAndOrganization());
            var rolesViewModel = _mapper.Map<IEnumerable<RoleDto>, IEnumerable<RoleViewModel>>(rolesDto);
            return Ok(rolesViewModel);
        }

        [HttpGet]
        [Route("GetPermissionGroups")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<IHttpActionResult> GetPermissionGroups()
        {
            var roleGroups = await _permissionService.GetGroupNamesAsync(GetUserAndOrganization().OrganizationId);
            var roleGroupsViewModel = _mapper.Map<IEnumerable<PermissionGroupDto>, IEnumerable<PermissionGroupViewModel>>(roleGroups);
            return Ok(roleGroupsViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        [Route("Get")]
        public async Task<IHttpActionResult> Get(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("roleId can't be empty");
            }

            var roleDetailsDto = await _roleService.GetRoleByIdAsync(GetUserAndOrganization(), roleId);
            var roleDetailsViewModel = _mapper.Map<RoleDetailsDto, RoleDetailsViewModel>(roleDetailsDto);
            return Ok(roleDetailsViewModel);
        }

        [HttpGet]
        [Route("GetUsersForAutoComplete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<IEnumerable<ApplicationUserViewModel>> GetUsersForAutoComplete(string s)
        {
            IEnumerable<ApplicationUserViewModel> applicationUserViewModels;

            if (!string.IsNullOrEmpty(s))
            {
                var applicationUsers = await _applicationUserRepository.Get(e => (e.FirstName + " " + e.LastName).Contains(s)).ToListAsync();

                applicationUserViewModels = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<ApplicationUserViewModel>>(applicationUsers);
            }
            else
            {
                return null;
            }

            return applicationUserViewModels;
        }

        [ValidationFilter]
        [Route("Post")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<HttpResponseMessage> Post([FromBody] RoleMiniViewModel roleViewModel)
        {
            if (await _roleRepository.Get(r => r.Name == roleViewModel.Name).AnyAsync())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Models.Role.Role.RoleNameExistsError);
            }

            roleViewModel.Id = Guid.NewGuid().ToString();

            var role = _mapper.Map<RoleMiniViewModel, ApplicationRole>(roleViewModel);
            role.CreatedTime = DateTime.UtcNow;
            role.OrganizationId = GetUserAndOrganization().OrganizationId;

            await _roleManager.CreateAsync(role);
            await AssignPermissionsToRoleAsync(roleViewModel, role);
            await AssignUsersToRole(roleViewModel);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK, role.Id);
        }

        [ValidationFilter]
        [Route("Put")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<HttpResponseMessage> Put([FromBody] RoleMiniViewModel roleViewModel)
        {
            if (_roleRepository.Get(r => r.Name == roleViewModel.Name && r.Id != roleViewModel.Id).Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Models.Role.Role.RoleNameExistsError);
            }

            var role = await _roleRepository.Get(r => r.Id == roleViewModel.Id, includeProperties: "Permissions").FirstAsync();
            _mapper.Map(roleViewModel, role);

            await AssignPermissionsToRoleAsync(roleViewModel, role);
            await AssignUsersToRole(roleViewModel);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK, role.Id);
        }

        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<HttpResponseMessage> Delete(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            await _roleManager.DeleteAsync(role);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("GetPaged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public async Task<PagedViewModel<RoleViewModel>> GetPaged(int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string s = "",
            string sort = "Name",
            string dir = "",
            string includeProperties = "")
        {
            var sortString = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var roles = await GetRolesAsync(sortString, includeProperties, s);

            var rolesViewModel = _mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(roles);

            var pagedList = await rolesViewModel.ToPagedListAsync(page, pageSize);

            var pagedModel = new PagedViewModel<RoleViewModel>
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        private async Task<IEnumerable<ApplicationRole>> GetRolesAsync(string sortString, string includeProperties, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return await _roleRepository.Get(orderBy: sortString, includeProperties: includeProperties).ToListAsync();
            }

            return await _roleRepository.Get(r => r.Name.Contains(s) || r.Organization.ShortName.Contains(s), orderBy: sortString, includeProperties: includeProperties).ToListAsync();
        }

        private async Task AssignPermissionsToRoleAsync(RoleMiniViewModel roleViewModel, ApplicationRole role)
        {
            var filteredPermissions = GetFilter(roleViewModel.Permissions.ToList());
            role.Permissions = await _permissionRepository.Get(filteredPermissions).ToListAsync();

            _roleRepository.Update(role);
            await _unitOfWork.SaveAsync();
        }

        private static Expression<Func<Permission, bool>> GetFilter(IList<PermissionGroupViewModel> permissions)
        {
            var adminControllers = permissions.Where(p => p.ActiveScope == PermissionScopes.Administration).Select(p => p.Name);
            var basicControllers = permissions.Where(p => p.ActiveScope == PermissionScopes.Basic).Select(p => p.Name);

            return p => adminControllers.Any(a => p.Name.StartsWith(a + "_")) || (basicControllers.Any(b => p.Name.StartsWith(b + "_")) && p.Scope == PermissionScopes.Basic);
        }

        private async Task AssignUsersToRole(RoleMiniViewModel roleViewModel)
        {
            var usersInModelIds = _mapper.Map<IEnumerable<ApplicationUserViewModel>, string[]>(roleViewModel.Users);
            var usersToAdd = await _applicationUserRepository.Get(u => u.Roles.Count(r => r.RoleId.Contains(roleViewModel.Id)) == 0 && usersInModelIds.Contains(u.Id)).ToListAsync();

            foreach (var user in usersToAdd)
            {
                var state = await _userManager.AddToRoleAsync(user.Id, roleViewModel.Name);
                if (!state.Succeeded)
                {
                    throw new SystemException(state.Errors.Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(", ", a)), sb => sb.ToString()));
                }
            }

            var usersToRemove = await _applicationUserRepository.Get(u => u.Roles.Count(r => r.RoleId.Contains(roleViewModel.Id)) == 1 && !usersInModelIds.Contains(u.Id)).ToListAsync();

            foreach (var user in usersToRemove)
            {
                var state = await _userManager.RemoveFromRoleAsync(user.Id, roleViewModel.Name);
                if (!state.Succeeded)
                {
                    throw new SystemException(state.Errors.Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(", ", a)), sb => sb.ToString()));
                }
            }
        }
    }
}
