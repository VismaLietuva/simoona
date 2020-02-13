using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.Authentification.Membership;
using Shrooms.DataTransferObjects.Models.Permissions;
using Shrooms.DataTransferObjects.Models.Roles;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Roles;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.API.Controllers
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
        private ICustomCache<string, IEnumerable<string>> _permissionsCache;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;

        private readonly IRoleService _roleService;

        public ShroomsRoleManager RoleManager { get; set; }
        public ShroomsUserManager UserManager { get; set; }

        public RolesController(
            IMapper mapper,
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

            RoleManager = roleManager;
            UserManager = userManager;
        }

        #region private methods

        private void AssignPermissionsToARole(RoleMiniViewModel roleViewModel, ApplicationRole role)
        {
            role.Permissions = _permissionRepository.Get(GetFilter(roleViewModel.Permissions)).ToList();
            _roleRepository.Update(role);
            _unitOfWork.Save();
        }

        private Expression<Func<Permission, bool>> GetFilter(IEnumerable<PermissionGroupViewModel> permissions)
        {
            var adminControllers = permissions.Where(p => p.ActiveScope == PermissionScopes.Administration).Select(p => p.Name);
            var basicControllers = permissions.Where(p => p.ActiveScope == PermissionScopes.Basic).Select(p => p.Name);

            return p => adminControllers.Any(a => p.Name.StartsWith(a + "_")) || (basicControllers.Any(b => p.Name.StartsWith(b + "_")) && p.Scope == PermissionScopes.Basic);
        }

        private void AssignUsersToRole(RoleMiniViewModel roleViewModel)
        {
            var usersInModelIds = _mapper.Map<IEnumerable<ApplicationUserViewModel>, string[]>(roleViewModel.Users);
            var usersToAdd = _applicationUserRepository.Get(u => u.Roles.Count(r => r.RoleId.Contains(roleViewModel.Id)) == 0 && usersInModelIds.Contains(u.Id)).ToList();

            usersToAdd.ForEach(u =>
            {
                var state = UserManager.AddToRole(u.Id, roleViewModel.Name);
                if (!state.Succeeded)
                {
                    throw new SystemException(state.Errors.Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(", ", a)), sb => sb.ToString()));
                }
            });

            var usersToRemove = _applicationUserRepository.Get(u => u.Roles.Count(r => r.RoleId.Contains(roleViewModel.Id)) == 1 && !usersInModelIds.Contains(u.Id)).ToList();

            usersToRemove.ForEach(u =>
            {
                var state = UserManager.RemoveFromRole(u.Id, roleViewModel.Name);
                if (!state.Succeeded)
                {
                    throw new SystemException(state.Errors.Aggregate(new StringBuilder(), (sb, a) => sb.AppendLine(string.Join(", ", a)), sb => sb.ToString()));
                }
            });
        }

        #endregion

        [HttpGet]
        [Route("GetRolesForAutocomplete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public IHttpActionResult GetRolesForAutoComplete(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return BadRequest("Search string can't be empty");
            }

            var rolesDTO = _roleService.GetRolesForAutocomplete(search, GetUserAndOrganization());
            var rolesViewModel = _mapper.Map<IEnumerable<RoleDTO>, IEnumerable<RoleViewModel>>(rolesDTO);
            return Ok(rolesViewModel);
        }

        [HttpGet]
        [Route("GetPermissionGroups")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public IHttpActionResult GetPermissionGroups()
        {
            var roleGroups = _permissionService.GetGroupNames(GetUserAndOrganization().OrganizationId);
            var roleGroupsViewModel = _mapper.Map<IEnumerable<PermissionGroupDTO>, IEnumerable<PermissionGroupViewModel>>(roleGroups);
            return Ok(roleGroupsViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        [Route("Get")]
        public IHttpActionResult Get(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("roleId can't be empty");
            }

            var roleDetailsDTO = _roleService.GetRoleById(GetUserAndOrganization(), roleId);
            var roleDetailsViewModel = _mapper.Map<RoleDetailsDTO, RoleDetailsViewModel>(roleDetailsDTO);
            return Ok(roleDetailsViewModel);
        }

        [HttpGet]
        [Route("GetUsersForAutoComplete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public IEnumerable<ApplicationUserViewModel> GetUsersForAutoComplete(string s)
        {
            IEnumerable<ApplicationUserViewModel> applicationUserViewModels;

            if (!string.IsNullOrEmpty(s))
            {
                var applicationUsers = _applicationUserRepository.Get(e => (e.FirstName + " " + e.LastName).Contains(s));

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
        public HttpResponseMessage Post([FromBody] RoleMiniViewModel roleViewModel)
        {
            if (_roleRepository.Get(r => r.Name == roleViewModel.Name).Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Models.Role.Role.RoleNameExistsError);
            }

            roleViewModel.Id = Guid.NewGuid().ToString();

            var role = _mapper.Map<RoleMiniViewModel, ApplicationRole>(roleViewModel);
            role.CreatedTime = DateTime.UtcNow;
            role.OrganizationId = GetUserAndOrganization().OrganizationId;

            RoleManager.Create(role);
            AssignPermissionsToARole(roleViewModel, role);
            AssignUsersToRole(roleViewModel);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK, role.Id);
        }

        [ValidationFilter]
        [Route("Put")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public HttpResponseMessage Put([FromBody] RoleMiniViewModel roleViewModel)
        {
            if (_roleRepository.Get(r => r.Name == roleViewModel.Name && r.Id != roleViewModel.Id).Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Models.Role.Role.RoleNameExistsError);
            }

            var role = _roleRepository.Get(r => r.Id == roleViewModel.Id, includeProperties: "Permissions").FirstOrDefault();
            _mapper.Map(roleViewModel, role);

            AssignPermissionsToARole(roleViewModel, role);
            AssignUsersToRole(roleViewModel);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK, role.Id);
        }

        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public HttpResponseMessage Delete(string roleId)
        {
            var role = RoleManager.FindById(roleId);

            if (role == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            RoleManager.Delete(role);
            _permissionsCache.Clear();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("GetPaged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        public PagedViewModel<RoleViewModel> GetPaged(int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string s = "",
            string sort = "Name", string dir = "", string includeProperties = "")
        {
            string sortString = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";

            var roles = GetRoles(sortString, includeProperties, s);

            var rolesViewModel = _mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(roles);

            var pagedList = rolesViewModel.ToPagedList(page, pageSize);

            var pagedModel = new PagedViewModel<RoleViewModel>
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        [Route("GetRoles")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Role)]
        private IEnumerable<ApplicationRole> GetRoles(string sortString, string includeProperties, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return _roleRepository.Get(orderBy: sortString, includeProperties: includeProperties);
            }

            return _roleRepository.Get(filter: r => r.Name.Contains(s) || r.Organization.ShortName.Contains(s), orderBy: sortString, includeProperties: includeProperties);
        }
    }
}
