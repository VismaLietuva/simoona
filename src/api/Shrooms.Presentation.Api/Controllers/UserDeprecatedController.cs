using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.DataTransferObjects.Models.Administration;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Exceptions.Exceptions.UserAdministration;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Domain.Services.Impersonate;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.Projects;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.UserService;
using Shrooms.Presentation.Common.Controllers.Kudos;
using Shrooms.Presentation.Common.Controllers.Wall;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;
using Shrooms.Presentation.WebViewModels.Models.Profile.JobPosition;
using Shrooms.Presentation.WebViewModels.Models.User;
using WebApi.OutputCache.V2;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("ApplicationUser")]
    public partial class UserDeprecatedController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;

        private readonly IRepository<QualificationLevel> _qualificationLevelRepository;
        private readonly IRepository<ApplicationRole> _rolesRepository;
        private readonly IRepository<Exam> _examsRepository;
        private readonly IRepository<Skill> _skillsRepository;
        private readonly IRepository<JobPosition> _jobPositionsRepository;
        private readonly IImpersonateService _impersonateService;
        private readonly IAdministrationUsersService _administrationUsersService;
        private readonly IPermissionService _permissionService;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IRoleService _roleService;
        private readonly ShroomsUserManager _userManager;
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;
        private readonly IProjectsService _projectService;
        private readonly IKudosService _kudosService;
        private readonly IPictureService _pictureService;
        private readonly IBlacklistService _blacklistService;

        public UserDeprecatedController(IMapper mapper,
            IUnitOfWork unitOfWork,
            ShroomsUserManager userManager,
            IImpersonateService impersonateService,
            IAdministrationUsersService administrationUsersService,
            IPermissionService permissionService,
            IOrganizationService organizationService,
            IUserService userService,
            ICustomCache<string, IEnumerable<string>> permissionsCache,
            IRoleService roleService,
            IProjectsService projectService,
            IKudosService kudosService,
            IPictureService pictureService,
            IBlacklistService blacklistService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _roomRepository = _unitOfWork.GetRepository<Room>();
            _applicationUserRepository = _unitOfWork.GetRepository<ApplicationUser>();
            _rolesRepository = unitOfWork.GetRepository<ApplicationRole>();
            _examsRepository = _unitOfWork.GetRepository<Exam>();
            _skillsRepository = _unitOfWork.GetRepository<Skill>();
            _jobPositionsRepository = _unitOfWork.GetRepository<JobPosition>();
            _qualificationLevelRepository = _unitOfWork.GetRepository<QualificationLevel>();
            _impersonateService = impersonateService;
            _administrationUsersService = administrationUsersService;
            _permissionService = permissionService;
            _organizationService = organizationService;
            _permissionsCache = permissionsCache;
            _userService = userService;
            _userManager = userManager;
            _roleService = roleService;
            _projectService = projectService;
            _kudosService = kudosService;
            _pictureService = pictureService;
            _blacklistService = blacklistService;
        }

        private async Task<bool> HasPermissionAsync(UserAndOrganizationDto userOrg, string permission)
        {
            return await _permissionService.UserHasPermissionAsync(userOrg, permission);
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        [InvalidateCacheOutput("GetKudosStats", typeof(KudosController))]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var canNotBeDeleted = await _kudosService.HasPendingKudosAsync(id);

            if (canNotBeDeleted)
            {
                return Content(HttpStatusCode.MethodNotAllowed, "Employee has pending kudos");
            }

            await _userService.DeleteAsync(id, GetUserAndOrganization());
            return Ok();
        }

        [Route("GetUsersAsExcel")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> GetUsersAsExcel()
        {
            var content = await _administrationUsersService.GetAllUsersExcelAsync("Users", GetOrganizationId());

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return ResponseMessage(result);
        }

        #region private methods

        private async Task UpdateRolesAsync(ApplicationUser user, IEnumerable<string> roleIds)
        {
            user.Roles.Clear();
            if (roleIds == null)
            {
                return;
            }

            foreach (var id in roleIds)
            {
                var role = await _rolesRepository.GetByIdAsync(id);
                if (role == null)
                {
                    throw new Exception(string.Format(Resources.Common.DoesNotExist + " Id: {1}", "Role", id));
                }

                user.Roles.Add(new IdentityUserRole { RoleId = id, UserId = user.Id });
            }
        }

        #endregion

        [Route("GetAll")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IEnumerable<ApplicationUserViewModel>> GetAll()
        {
            var applicationUsers = await _applicationUserRepository.Get().ToListAsync();
            return _mapper.Map<IEnumerable<ApplicationUser>, List<ApplicationUserViewModel>>(applicationUsers);
        }

        [Route("Get")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> Get(string id, string includeProperties = "")
        {
            var user = await _applicationUserRepository.Get(e => e.Id == id, includeProperties: includeProperties).FirstOrDefaultAsync();
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName));
            }

            var model = _mapper.Map<ApplicationUser, ApplicationUserViewModel>(user);
            model.IsAdmin = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), "APPLICATIONUSER_ADMINISTRATION");

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetByRoom")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IEnumerable<ApplicationUserViewModel>> GetByRoomAsync(int roomId, string includeProperties = "")
        {
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var applicationUsers = _applicationUserRepository
                .Get(e => e.Room.Id == roomId, includeProperties: includeProperties)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId));

            var applicationUsersViewModel = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<ApplicationUserViewModel>>(applicationUsers);
            return applicationUsersViewModel;
        }

        [Route("GetByFloor")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IEnumerable<ApplicationUserViewModel>> GetByFloorAsync(int floorId, string includeProperties = "")
        {
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var applicationUsers = _applicationUserRepository
                .Get(e => e.Room.FloorId == floorId, includeProperties: includeProperties)
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId));

            return _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<ApplicationUserViewModel>>(applicationUsers);
        }

        [Route("GetByUserName")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<ApplicationUserViewModel> GetByUserName(string userName, string includeProperties = "")
        {
            var applicationUser = await _applicationUserRepository.Get(e => e.UserName == userName, includeProperties: includeProperties).FirstOrDefaultAsync();
            return _mapper.Map<ApplicationUser, ApplicationUserViewModel>(applicationUser);
        }

        [Route("GetPaged")]
        [HttpGet, HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<PagedViewModel<AdministrationUserDto>> GetPaged(int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string s = "",
            string sort = "LastName",
            string dir = "",
            string filter = null,
            string includeProperties = "")
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";
            FilterDto[] filterModel = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterModel = new JavaScriptSerializer().Deserialize<FilterDto[]>(filter);
                includeProperties += (includeProperties != string.Empty ? "," : string.Empty) + "Projects";

                filterModel ??= new[] { new JavaScriptSerializer().Deserialize<FilterDto>(filter) };
            }

            var administrationUsersDto = await _administrationUsersService.GetAllUsersAsync(sortQuery, s, filterModel, includeProperties);

            return await PagedViewModel(page, pageSize, administrationUsersDto);
        }

        private static async Task<PagedViewModel<T>> PagedViewModel<T>(int page, int pageSize, IEnumerable<T> officeUsers)
            where T : class
        {
            var officeUserPagedViewModel = await officeUsers.ToPagedListAsync(page, pageSize);

            var pagedModel = new PagedViewModel<T>
            {
                PagedList = officeUserPagedViewModel,
                PageCount = officeUserPagedViewModel.PageCount,
                ItemCount = officeUserPagedViewModel.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        [Route("GetJobTitleForAutoComplete")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<List<string>> GetJobTitleForAutoComplete(string query)
        {
            var userOrg = GetUserAndOrganization();
            var q = query.ToLowerInvariant();

            var jobPositions = await _jobPositionsRepository
                .Get(p => p.OrganizationId == userOrg.OrganizationId && p.Title.ToLower().Contains(q))
                .Select(p => p.Title)
                .OrderBy(p => p)
                .ToListAsync();

            return jobPositions;
        }

        [Route("GetDetails/{id}")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetDetails(string id)
        {
            var user = await _applicationUserRepository
                .Get(u => u.Id == id, includeProperties: WebApiConstants.PropertiesForUserDetails)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[]
                {
                    string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName)
                });
            }

            var model = _mapper.Map<ApplicationUserDetailsViewModel>(user);

            await InfoWithAdditionalPermissionsAsync(user, model);

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        private async Task InfoWithAdditionalPermissionsAsync(ApplicationUser user, ApplicationUserDetailsViewModel model)
        {
            var userOrg = GetUserAndOrganization();
            var permissions = (await _permissionService.GetUserPermissionsAsync(userOrg.UserId, userOrg.OrganizationId)).ToList();

            var hasApplicationUserPermission = permissions.Contains(AdministrationPermissions.ApplicationUser);
            var hasBlacklistPermission = permissions.Contains(BasicPermissions.Blacklist);

            var usersProfile = User.Identity.GetUserId() == user.Id;

            if (hasApplicationUserPermission)
            {
                var roles = await GetUserRolesAsync(user.Id);
                model.Roles = _mapper.Map<IEnumerable<ApplicationRoleMiniViewModel>>(roles);
            }

            if (!hasApplicationUserPermission && !usersProfile)
            {
                model.BirthDay = BirthdayDateTimeHelper.RemoveYear(model.BirthDay);
                model.PhoneNumber = null;
            }

            if ((hasBlacklistPermission || usersProfile) && _blacklistService.TryFindActiveBlacklistUserEntry(user.BlacklistEntries, out var blacklistUserDto))
            {
                model.BlacklistEntry = _mapper.Map<BlacklistUserDto, BlacklistUserViewModel>(blacklistUserDto);
            }
        }

        [HttpPut]
        [Route("ConfirmUser")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> ConfirmUser(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = User.Identity.GetOrganizationId(),
                UserId = User.Identity.GetUserId()
            };

            try
            {
                await _administrationUsersService.ConfirmNewUserAsync(id, userAndOrg);
                return Ok();
            }
            catch (UserAdministrationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("GetUserProfile/{id}")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetUserProfile(string id)
        {
            var user = await _applicationUserRepository
                .Get(e => e.Id == id,
                    includeProperties:
                    "Roles,ManagedUsers,Manager,Room,Room.RoomType,Room.Floor,Room.Floor.Office,RoomToConfirm,RoomToConfirm.RoomType,RoomToConfirm.Floor,RoomToConfirm.Floor.Office,Projects,Organization,Certificates,WorkingHours,Skills,QualificationLevel,Exams")
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[]
                {
                    string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName)
                });
            }

            var model = new ApplicationUserProfileViewModel
            {
                Id = user.Id,
                PersonalInfo = await MapPersonalInfoAsync(user),
                JobInfo = await MapJobInfoAsync(user),
                OfficeInfo = MapOfficeInfo(user),
                ShroomsInfo = MapShroomsInfo(user)
            };

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetUserProfile/{id}/Personal")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetUserPersonalInfo(string id)
        {
            if (!(await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser) || id == User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var user = await _applicationUserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            var model = await MapPersonalInfoAsync(user);

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetUserProfile/{id}/Job")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetUserJobInfo(string id)
        {
            if (!(await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser) || id == User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var user = await _applicationUserRepository.Get(u => u.Id == id, includeProperties: WebApiConstants.PropertiesForUserJobInfo).FirstOrDefaultAsync();
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            var model = await MapJobInfoAsync(user);
            var orgId = GetUserAndOrganization().OrganizationId;

            model.JobPositions = await _jobPositionsRepository
                .Get(p => p.OrganizationId == orgId)
                .Select(p => new JobPositionViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    IsSelected = p.Id == user.JobPositionId
                })
                .OrderBy(p => p.Title)
                .ToListAsync();

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetUserProfile/{id}/Office")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetUserOfficeInfo(string id)
        {
            if (!(await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser) || id == User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var user = await _applicationUserRepository.Get(u => u.Id == id, includeProperties: WebApiConstants.PropertiesForUserOfficeInfo).FirstOrDefaultAsync();
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            var model = MapOfficeInfo(user);
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetUserProfile/{id}/Shrooms")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetUserShroomsInfo(string id)
        {
            if (!(await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser) || id == User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var user = await _applicationUserRepository.Get(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            var model = MapShroomsInfo(user);

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [Route("GetProfile")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetProfile()
        {
            return await GetUserProfile(User.Identity.GetUserId());
        }

        [Route("GetProfile/Personal")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetPersonalInfo()
        {
            return await GetUserPersonalInfo(User.Identity.GetUserId());
        }

        [Route("GetProfile/Job")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetJobInfo()
        {
            return await GetUserJobInfo(User.Identity.GetUserId());
        }

        [Route("GetProfile/Office")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> GetOfficeInfo()
        {
            return await GetUserOfficeInfo(User.Identity.GetUserId());
        }

        [Route("GetPartTimeHoursOptions")]
        [AllowAnonymous]
        public Array GetPartTimeHoursOptions()
        {
            return Enum.GetValues(typeof(PartTimeHoursOptions));
        }

        [HttpPut]
        [Route("PutExams")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> PutExams(ApplicationUserExamsPostModel userExams)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetUserAndOrganization().UserId;
            var isAdministrator = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser);
            if (!isAdministrator && userExams.UserId != currentUserId)
            {
                return BadRequest();
            }

            var applicationUser = await _applicationUserRepository.Get(u => u.Id == userExams.UserId, includeProperties: "Exams").FirstOrDefaultAsync();
            if (applicationUser == null)
            {
                var errorMessage = string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName);
                return Content(HttpStatusCode.NotFound, errorMessage);
            }

            applicationUser.Exams = await _examsRepository.Get(e => userExams.ExamIds.Contains(e.Id)).ToListAsync();
            _applicationUserRepository.Update(applicationUser);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [ValidationFilter]
        [Route("PutJobInfo")]
        [HttpPut]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> PutJobInfo(ApplicationUserPutJobInfoViewModel model)
        {
            var userOrg = GetUserAndOrganization();
            var editorIsAdministrator = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.ApplicationUser);
            if (editorIsAdministrator && !model.EmploymentDate.HasValue)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (!(editorIsAdministrator || model.Id == User.Identity.GetUserId()))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var validatedModelInfo = await ValidateModelInfoAsync(model);
            if (!validatedModelInfo.IsSuccessStatusCode)
            {
                return validatedModelInfo;
            }

            var applicationUser = await _applicationUserRepository.Get(u => u.Id == model.Id, includeProperties: "Roles,Projects,Skills,WorkingHours").FirstOrDefaultAsync();

            if (!editorIsAdministrator)
            {
                model.EmploymentDate = applicationUser?.EmploymentDate;
            }

            if (applicationUser == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName));
            }

            _mapper.Map(model, applicationUser);

            applicationUser.Skills = await _skillsRepository.Get(s => model.SkillIds.Contains(s.Id)).ToListAsync();

            if (editorIsAdministrator && model.RoleIds != null)
            {
                await UpdateRolesAsync(applicationUser, model.RoleIds);
            }

            if (applicationUser.WorkingHours?.OrganizationId == 0)
            {
                applicationUser.WorkingHours.OrganizationId = GetUserAndOrganization().OrganizationId;
            }

            await _projectService.AddProjectsToUserAsync(applicationUser.Id, model.ProjectIds, userOrg);

            await _unitOfWork.SaveAsync();
            _permissionsCache.TryRemoveEntry(applicationUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<HttpResponseMessage> ValidateModelInfoAsync(ApplicationUserPutJobInfoViewModel model)
        {
            if (model.ManagerId != null)
            {
                var managerRole = await _rolesRepository.Get().FirstOrDefaultAsync(role => role.Name == Roles.Manager);

                var manager = await _applicationUserRepository
                    .Get(x => x.Id == model.ManagerId && x.Roles.Any(y => y.RoleId == managerRole.Id))
                    .FirstOrDefaultAsync();

                if (manager == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        string.Format(Resources.Common.DoesNotExist + " Id: {1}", Resources.Models.ApplicationUser.ApplicationUser.Manager, model.ManagerId));
                }

                if (!await _projectService.ValidateManagerIdAsync(model.Id, model.ManagerId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, Resources.Common.WrongManager);
                }
            }

            if (model.QualificationLevelId == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            var qualificationLevel = await _qualificationLevelRepository.GetByIdAsync(model.QualificationLevelId);
            if (qualificationLevel == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound,
                    string.Format(Resources.Common.DoesNotExist + " Id: {1}", Resources.Models.ApplicationUser.ApplicationUser.QualificationLevelName, model.QualificationLevelId));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("PutPersonalInfo")]
        [ValidationFilter]
        [HttpPut]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [InvalidateCacheOutput(nameof(WallWidgetsController.GetKudosBasketWidgetAsync), typeof(WallWidgetsController))]
        public async Task<HttpResponseMessage> PutPersonalInfo(ApplicationUserPutPersonalInfoViewModel model)
        {
            var validatedModel = await ValidateModelAsync(model);
            if (!validatedModel.IsSuccessStatusCode)
            {
                return validatedModel;
            }

            var userOrg = GetUserAndOrganization();
            var user = await _applicationUserRepository.GetByIdAsync(model.Id);

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            if ((user.FirstName != model.FirstName || user.LastName != model.LastName) && !await HasPermissionAsync(userOrg, AdministrationPermissions.ApplicationUser))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            if (await _applicationUserRepository.Get(u => u.Email == model.Email && u.Id != user.Id).AnyAsync())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { string.Format(Resources.Models.ApplicationUser.ApplicationUser.EmailAlreadyExsists) });
            }

            if (user.PictureId != model.PictureId && !string.IsNullOrEmpty(user.PictureId))
            {
                await _pictureService.RemoveImageAsync(user.PictureId, userOrg.OrganizationId);
            }

            _mapper.Map(model, user);
            _applicationUserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            if (!User.IsInRole(Roles.NewUser) || !await _userManager.IsInRoleAsync(user.Id, Roles.FirstLogin))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            await _userManager.RemoveFromRoleAsync(User.Identity.GetUserId(), Roles.FirstLogin);

            await _administrationUsersService.NotifyAboutNewUserAsync(user, userOrg.OrganizationId);
            var requiresConfirmation = await _organizationService.RequiresUserConfirmationAsync(userOrg.OrganizationId);

            if (!requiresConfirmation)
            {
                await _administrationUsersService.ConfirmNewUserAsync(userOrg.UserId, userOrg);
            }

            var response = new { requiresConfirmation };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        private async Task<HttpResponseMessage> ValidateModelAsync(ApplicationUserPutPersonalInfoViewModel model)
        {
            if (!await CanAccessAsync(model))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            if (model.BirthDay != null)
            {
                if (model.BirthDay.Value.Year < WebApiConstants.LowestBirthdayYear)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { string.Format(Resources.Common.BirthdayDateIsTooOld) });
                }

                if (model.BirthDay > DateTime.UtcNow)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { string.Format(Resources.Common.BirthdayDateValidationError) });
                }
            }

            if (!await _organizationService.IsOrganizationHostValidAsync(model.Email, Request.GetRequestedTenant()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { string.Format(Resources.Models.ApplicationUser.ApplicationUser.WrongEmailDomain) });
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("PutOfficeInfo")]
        [ValidationFilter]
        [HttpPut]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> PutOfficeInfo(ApplicationUserPutOfficeInfoViewModel model)
        {
            var userId = GetUserAndOrganization().UserId;
            var isUserAdmin = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser);
            if (!isUserAdmin && userId != model.Id)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist + " Id: " + model.RoomId, Resources.Models.Room.Room.EntityName) });
            }

            var user = await _applicationUserRepository.GetByIdAsync(model.Id);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            if (user.RoomId != model.RoomId)
            {
                user.RoomId = model.RoomId;
                user.SittingPlacesChanged++;
            }

            _applicationUserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("PutShroomsInfo")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<HttpResponseMessage> PutShroomsInfo(ApplicationUserShroomsInfoViewModel model)
        {
            if (!await CanAccessAsync(model))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            var user = await _applicationUserRepository.Get(u => u.Id == model.Id).FirstOrDefaultAsync();
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { string.Format(Resources.Common.DoesNotExist, Resources.Models.ApplicationUser.ApplicationUser.EntityName) });
            }

            _mapper.Map(model, user);

            _applicationUserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("GetForAutoComplete")]
        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IEnumerable<ApplicationUserAutoCompleteViewModel>> GetForAutoComplete(string s, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<ApplicationUserAutoCompleteViewModel>();
            }

            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            s = s.ToLowerInvariant();
            var users = await _applicationUserRepository
                .Get(u => u.UserName.ToLower().StartsWith(s)
                          || (u.FirstName != null && u.FirstName.ToLower().StartsWith(s))
                          || (u.LastName != null && u.LastName.ToLower().StartsWith(s))
                          || (u.FirstName != null && u.LastName != null && (u.FirstName.ToLower() + " " + u.LastName.ToLower()).StartsWith(s)))
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId))
                .OrderBy(u => u.Id)
                .ToPagedListAsync(1, pageSize);

            return _mapper.Map<IEnumerable<ApplicationUserAutoCompleteViewModel>>(users);
        }

        [HttpGet]
        [Route("GetManagersForAutoComplete")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IEnumerable<ApplicationUserAutoCompleteViewModel>> GetManagersForAutoComplete(string s, string userId = null, int pageSize = WebApiConstants.DefaultAutocompleteListSize)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<ApplicationUserAutoCompleteViewModel>();
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = User.Identity.GetUserId();
            }

            s = s.ToLowerInvariant();

            var managerRole = await _rolesRepository.Get().FirstOrDefaultAsync(role => role.Name == Roles.Manager);

            if (WebApiConstants.OrganizationManagerUsername.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var managersWithMantas = await _applicationUserRepository
                    .Get(m => m.Roles.Any(r => r.RoleId == managerRole.Id) && (m.UserName.ToLower().StartsWith(s)
                                                                               || (m.FirstName != null && m.FirstName.ToLower().StartsWith(s))
                                                                               || (m.LastName != null && m.LastName.ToLower().StartsWith(s))
                                                                               || (m.FirstName != null && m.LastName != null && (m.FirstName.ToLower() + " " + m.LastName.ToLower()).StartsWith(s))))
                    .OrderBy(u => u.Id)
                    .ToPagedListAsync(1, pageSize);

                return _mapper.Map<IEnumerable<ApplicationUserAutoCompleteViewModel>>(managersWithMantas);
            }

            var managers = await _applicationUserRepository
                .Get(m => m.Roles.Any(r => r.RoleId == managerRole.Id) && (m.Id != userId) && (m.UserName.ToLower().StartsWith(s)
                                                                                               || (m.FirstName != null && m.FirstName.ToLower().StartsWith(s))
                                                                                               || (m.LastName != null && m.LastName.ToLower().StartsWith(s))
                                                                                               || (m.FirstName != null && m.LastName != null &&
                                                                                                   (m.FirstName.ToLower() + " " + m.LastName.ToLower()).StartsWith(s))))
                .OrderBy(u => u.Id)
                .ToPagedListAsync(1, pageSize);

            return _mapper.Map<IEnumerable<ApplicationUserAutoCompleteViewModel>>(managers);
        }

        [HttpPut]
        [Route("CompleteTutorial")]
        public async Task<IHttpActionResult> SetUserTutorialStatusToComplete()
        {
            await _administrationUsersService.SetUserTutorialStatusToCompleteAsync(GetUserAndOrganization().UserId);
            return Ok();
        }

        [HttpGet]
        [Route("TutorialStatus")]
        public async Task<IHttpActionResult> GetUserTutorialStatus()
        {
            var tutorialStatus = await _administrationUsersService.GetUserTutorialStatusAsync(GetUserAndOrganization().UserId);
            return Ok(tutorialStatus);
        }

        private async Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(string userId)
        {
            return await _rolesRepository.Get(r => r.Users.Any(u => u.UserId == userId)).ToListAsync();
        }

        public async Task<ApplicationUserPersonalInfoViewModel> MapPersonalInfoAsync(ApplicationUser user)
        {
            var personalInfo = _mapper.Map<ApplicationUserPersonalInfoViewModel>(user);

            var canAccessFullProfile = await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser) || User.Identity.GetUserId() == user.Id;

            if (!canAccessFullProfile && !personalInfo.ShowBirthDay)
            {
                personalInfo.ShowableBirthDay = personalInfo.BirthDay != null ? $"****-{personalInfo.BirthDay?.ToString("MM-dd")}" : "";
                personalInfo.BirthDay = null;
            }

            return personalInfo;
        }

        private async Task<ApplicationUserJobInfoViewModel> MapJobInfoAsync(ApplicationUser user)
        {
            var jobInfo = _mapper.Map<ApplicationUserJobInfoViewModel>(user);
            var roles = await _rolesRepository.Get(r => r.Users.Any(u => u.UserId == user.Id)).ToListAsync();
            jobInfo.Roles = _mapper.Map<IEnumerable<ApplicationRoleMiniViewModel>>(roles);

            return jobInfo;
        }

        private ApplicationUserOfficeInfoViewModel MapOfficeInfo(ApplicationUser user)
        {
            var officeInfo = _mapper.Map<ApplicationUserOfficeInfoViewModel>(user);
            return officeInfo;
        }

        private ApplicationUserShroomsInfoViewModel MapShroomsInfo(ApplicationUser user)
        {
            return _mapper.Map<ApplicationUserShroomsInfoViewModel>(user);
        }

        private async Task<bool> CanAccessAsync(ApplicationUserBaseViewModel model)
        {
            return User.Identity.GetUserId() == model.Id || await _permissionService.UserHasPermissionAsync(GetUserAndOrganization(), AdministrationPermissions.ApplicationUser);
        }
    }
}
