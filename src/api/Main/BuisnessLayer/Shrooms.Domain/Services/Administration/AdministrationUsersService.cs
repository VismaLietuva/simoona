using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Shrooms.Authentification.Membership;
using Shrooms.Constants.Authentication;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Administration;
using Shrooms.Domain.Services.Email.AdministrationUsers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Picture;
using Shrooms.DomainServiceValidators.Validators.UserAdministration;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.ExcelGenerator;
using userRes = Shrooms.Resources.Models.ApplicationUser.ApplicationUser;

namespace Shrooms.Domain.Services.Administration
{
    public class AdministrationUsersService : IAdministrationUsersService
    {
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<ApplicationRole> _rolesRepository;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IDbSet<EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly IDbSet<WallMember> _wallUsersDbSet;
        private readonly ShroomsUserManager _userManager;
        private readonly IUserAdministrationValidator _userAdministrationValidator;
        private readonly IOrganizationService _organizationService;
        private readonly IPictureService _pictureService;
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAdministrationNotificationService _notificationService;
        private readonly IUnitOfWork2 _uow;

        public AdministrationUsersService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IUnitOfWork2 uow,
            IUserAdministrationValidator userAdministrationValidator,
            ShroomsUserManager userManager,
            IOrganizationService organizationService,
            IPictureService pictureService,
            IDbContext context,
            IAdministrationNotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _rolesRepository = unitOfWork.GetRepository<ApplicationRole>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
            _wallsDbSet = uow.GetDbSet<EntityModels.Models.Multiwall.Wall>();
            _wallUsersDbSet = uow.GetDbSet<WallMember>();
            _organizationDbSet = uow.GetDbSet<Organization>();
            _userManager = userManager;
            _userAdministrationValidator = userAdministrationValidator;
            _organizationService = organizationService;
            _pictureService = pictureService;
            _context = context;
            _notificationService = notificationService;
        }

        public byte[] GetAllUsersExcel()
        {
            var applicationUsers = _usersDbSet
                .Include(user => user.WorkingHours)
                .Include(user => user.JobPosition)
                .Where(user => user.OrganizationId == 2)
                .ToList();

            using (var printer = new ExcelGenerator("Users"))
            {
                printer.CenterColumns(2, 6);
                printer.AddHeaderRow(HeaderRow());

                foreach (var user in applicationUsers)
                {
                    printer.AddRow(UserToUserRow(user));
                }

                return printer.Generate();
            }
        }

        public bool UserIsSoftDeleted(string email)
        {
            var shroomsContext = _context as ShroomsDbContext;

            if (shroomsContext == null)
            {
                throw new ArgumentNullException(nameof(shroomsContext));
            }

            var user = shroomsContext
                .Users
                .SqlQuery("Select * From [dbo].[AspNetUsers] Where Email = @email", new SqlParameter("@email", email))
                .SingleOrDefault();

            return user != null;
        }

        public void RestoreUser(string email)
        {
            var shroomsContext = _context as ShroomsDbContext;

            if (shroomsContext == null)
            {
                throw new ArgumentNullException(nameof(shroomsContext));
            }

            shroomsContext
                .Database
                .ExecuteSqlCommand("UPDATE [dbo].[AspNetUsers] SET[IsDeleted] = '0' WHERE Email = @email", new SqlParameter("@email", email));

            var user = _userManager.FindByEmail(email);
            AddNewUserRoles(user.Id);
        }

        public async Task AddProviderImage(string userId, ClaimsIdentity externalIdentity)
        {
            var user = _usersDbSet.First(u => u.Id == userId);
            if (user.PictureId == null && externalIdentity.FindFirst("picture") != null)
            {
                byte[] data = data = await new WebClient().DownloadDataTaskAsync(externalIdentity.FindFirst("picture").Value);
                user.PictureId = await _pictureService.UploadFromStream(new MemoryStream(data), "image/jpeg", Guid.NewGuid() + ".jpg", user.OrganizationId);
                _uow.SaveChanges(userId);
            }
        }

        public void NotifyAboutNewUser(ApplicationUser user, int orgId)
        {
            _notificationService.NotifyAboutNewUser(user, orgId);
        }

        public async Task SendUserPasswordResetEmail(ApplicationUser user, string organizationName)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);

            _notificationService.SendUserResetPasswordEmail(user, token, organizationName);
        }

        public async Task SendUserVerificationEmail(ApplicationUser user, string orgazinationName)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);

            _notificationService.SendUserVerificationEmail(user, token, orgazinationName);
        }

        public void ConfirmNewUser(string userId, UserAndOrganizationDTO userAndOrg)
        {
            var applicationUser = _usersDbSet.First(user => user.Id == userId);
            _userAdministrationValidator.CheckIfEmploymentDateIsSet(applicationUser.EmploymentDate);

            var hasRole = _userManager.IsInRole(userId, Host.Contracts.Constants.Roles.FirstLogin);
            _userAdministrationValidator.CheckIfUserHasFirstLoginRole(hasRole);

            var addRoleResult = _userManager.AddToRole(userId, Host.Contracts.Constants.Roles.User);
            var removeRoleResult = _userManager.RemoveFromRole(userId, Host.Contracts.Constants.Roles.NewUser);

            _userAdministrationValidator.CheckForAddingRemovingRoleErrors(addRoleResult.Errors, removeRoleResult.Errors);
            _notificationService.SendConfirmedNotificationEmail(applicationUser.Email, userAndOrg);

            SetTutorialStatus(applicationUser, false);

            AddUserToMainWall(userId);
            _uow.SaveChanges(userAndOrg.UserId);
        }

        public async Task<IdentityResult> CreateNewUserWithExternalLogin(ExternalLoginInfo info, string requestedOrganization)
        {
            var externalIdentity = info.ExternalIdentity;
            var userSettings =
                _organizationDbSet.Where(o => o.ShortName == requestedOrganization)
                .Select(u => new { u.CultureCode, u.TimeZone })
                .First();

            var user = new ApplicationUser
            {
                UserName = externalIdentity.FindFirst(ClaimTypes.Email).Value,
                Email = externalIdentity.FindFirst(ClaimTypes.Email).Value,
                FirstName = externalIdentity.FindFirst(ClaimTypes.GivenName).Value,
                LastName = externalIdentity.FindFirst(ClaimTypes.Surname).Value,
                OrganizationId = _organizationService.GetOrganizationByName(requestedOrganization).Id,
                EmploymentDate = DateTime.UtcNow,
                CultureCode = userSettings.CultureCode ?? BusinessLayerConstants.DefaultCulture,
                TimeZone = userSettings.TimeZone,
                NotificationsSettings = null
            };

            if (externalIdentity.FindFirst("picture") != null)
            {
                byte[] data = data = await new WebClient().DownloadDataTaskAsync(externalIdentity.FindFirst("picture").Value);
                user.PictureId = await _pictureService.UploadFromStream(new MemoryStream(data), "image/jpeg", Guid.NewGuid() + ".jpg", user.OrganizationId);
            }

            var result = _userManager.Create(user);
            if (!result.Succeeded)
            {
                return result;
            }

            AddNewUserRoles(user.Id);
            return result;
        }

        public async Task<IdentityResult> CreateNewUser(ApplicationUser user, string password, string requestedOrganization)
        {
            var userSettings =
                _organizationDbSet.Where(o => o.ShortName == requestedOrganization)
                .Select(u => new { u.CultureCode, u.TimeZone })
                .First();

            user.OrganizationId = _organizationService.GetOrganizationByName(requestedOrganization).Id;
            user.EmploymentDate = DateTime.UtcNow;
            user.CultureCode = userSettings.CultureCode ?? BusinessLayerConstants.DefaultCulture;
            user.TimeZone = userSettings.TimeZone;
            user.NotificationsSettings = null;

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return result;
            }

            var userLoginInfo = new UserLoginInfo(AuthenticationConstants.InternalLoginProvider, user.Id);
            var addLoginResult = await _userManager.AddLoginAsync(user.Id, userLoginInfo);
            if (!addLoginResult.Succeeded)
            {
                return addLoginResult;
            }

            AddNewUserRoles(user.Id);

            await SendUserVerificationEmail(user, requestedOrganization);

            return result;
        }

        public bool HasExistingExternalLogin(string email, string loginProvider)
        {
            var user = _userManager.FindByEmail(email);
            if (user == null)
            {
                return false;
            }

            var hasLogin = user.Logins.Any(login => login.LoginProvider == loginProvider);
            return hasLogin;
        }

        public bool UserEmailExists(string email)
        {
            var user = _userManager.FindByEmail(email);
            return user != null;
        }

        public IEnumerable<AdministrationUserDTO> GetAllUsers(string sortQuery, string search, FilterDTO[] filterModel, string includeProperties)
        {
            includeProperties += (includeProperties != string.Empty ? "," : string.Empty) + "Roles,Skills,JobPosition,Projects";
            var applicationUsers = _applicationUserRepository
                .Get(GenerateQuery(search), orderBy: sortQuery.Contains(Host.Contracts.Constants.Roles.NewUser) ? string.Empty : sortQuery, includeProperties: includeProperties)
                .ToList();

            var administrationUserDto = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<AdministrationUserDTO>>(applicationUsers);

            SetNewUsersValues(administrationUserDto, applicationUsers);

            if (filterModel != null)
            {
                administrationUserDto = GetFilteredResults(filterModel, administrationUserDto);
            }

            if (sortQuery.StartsWith(Host.Contracts.Constants.Roles.NewUser))
            {
                administrationUserDto = sortQuery.EndsWith("asc") ? administrationUserDto.OrderBy(u => u.IsNewUser) :
                    administrationUserDto.OrderByDescending(u => u.IsNewUser);
            }

            return administrationUserDto;
        }

        public void SetUserTutorialStatusToComplete(string userId)
        {
            var applicationUser = _usersDbSet.First(user => user.Id == userId);
            SetTutorialStatus(applicationUser, true);
            _uow.SaveChanges(userId);
        }

        public bool GetUserTutorialStatus(string userId)
        {
            return _usersDbSet.First(user => user.Id == userId).IsTutorialComplete;
        }

        public void AddProviderEmail(string userId, string provider, string email)
        {
            var user = _usersDbSet.First(u => u.Id == userId);
            if (provider == AuthenticationConstants.GoogleLoginProvider)
            {
                user.GoogleEmail = email;
            }

            if (provider == AuthenticationConstants.FacebookLoginProvider)
            {
                user.FacebookEmail = email;
            }

            _uow.SaveChanges(userId);
        }

        private static Expression<Func<ApplicationUser, bool>> GenerateQuery(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var searchKeyWords = s.Split(BusinessLayerConstants.SearchSplitter).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return e => searchKeyWords.Count(n =>
                e.UserName.Contains(n) ||
                e.FirstName.Contains(n) ||
                e.LastName.Contains(n) ||
                e.JobPosition.Title.Contains(n) ||
                e.Skills.Select(skill => skill.Title).Contains(n) ||
                e.Projects.Any(p => p.Name.Contains(n)) ||
                e.QualificationLevel.Name.Contains(n) ||
                e.Room.Name.Contains(n) ||
                e.Room.Number.Contains(n) ||
                e.Room.RoomType.Name.Contains(n) ||
                e.Room.Floor.Name.Contains(n) ||
                e.Room.Floor.Office.Name.Contains(n)) == searchKeyWords.Count();
        }

        private static List<string> HeaderRow()
        {
            var result = new List<string>
            {
                userRes.FirstName,
                userRes.LastName,
                userRes.Birthday,
                userRes.JobTitle,
                userRes.PhoneNumber,
                userRes.WorkingHours,
                userRes.HasPicture
            };

            return result;
        }

        private static List<object> UserToUserRow(ApplicationUser user)
        {
            var result = new List<object>
            {
                user.FirstName,
                user.LastName,
                user.BirthDay,
                user.JobPosition == null ? string.Empty : user.JobPosition.Title,
                StringToLong(user.PhoneNumber),
                user.WorkingHours != null
                    ? $"{user.WorkingHours.StartTime}-{user.WorkingHours.EndTime}"
                    : string.Empty,
                user.PictureId != null ? Resources.Common.Yes : Resources.Common.No
            };

            return result;
        }

        private static long? StringToLong(string str)
        {
            var regex = new Regex(@"\D");

            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            long? number = null;
            var numbers = regex.Replace(str, string.Empty);
            if (!string.IsNullOrEmpty(numbers))
            {
                number = Convert.ToInt64(numbers);
            }

            return number;
        }

        private static void SetTutorialStatus(ApplicationUser user, bool tutorialStatus)
        {
            user.IsTutorialComplete = tutorialStatus;
        }

        private void AddNewUserRoles(string id)
        {
            _userManager.AddToRole(id, Host.Contracts.Constants.Roles.NewUser);
            _userManager.AddToRole(id, Host.Contracts.Constants.Roles.FirstLogin);
        }

        private void SetNewUsersValues(IEnumerable<AdministrationUserDTO> administrationUserDto, IEnumerable<ApplicationUser> applicationUsers)
        {
            var newUserRole = _rolesRepository.Get(x => x.Name == Host.Contracts.Constants.Roles.NewUser).Select(x => x.Id).FirstOrDefault();

            var usersWaitingForConfirmationIds =
                applicationUsers.Where(x => x.Roles.Any(y => y.RoleId == newUserRole)).Select(x => x.Id).ToList();

            foreach (var user in usersWaitingForConfirmationIds)
            {
                administrationUserDto.FirstOrDefault(x => x.Id == user).IsNewUser = true;
            }
        }

        private IEnumerable<AdministrationUserDTO> GetFilteredResults(IEnumerable<FilterDTO> filterModel, IEnumerable<AdministrationUserDTO> applicationUsersViewModel)
        {
            foreach (var filterViewModel in filterModel)
            {
                switch (filterViewModel.Key.ToLowerInvariant())
                {
                    case "jobtitle":
                        applicationUsersViewModel =
                            applicationUsersViewModel.Where(
                                e =>
                                filterViewModel.Values.Any(
                                    v =>
                                    !string.IsNullOrWhiteSpace(e.JobTitle)
                                    && e.JobTitle.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0));
                        break;

                    case "projects":
                        applicationUsersViewModel =
                            applicationUsersViewModel.Where(
                                e =>
                                filterViewModel.Values.Any(
                                    v =>
                                    e.Projects != null
                                    && e.Projects.Any(
                                        p =>
                                        !string.IsNullOrWhiteSpace(p.Name)
                                        && (p.Name.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0))));
                        break;

                    case "skills":
                        applicationUsersViewModel =
                            applicationUsersViewModel.Where(
                                e =>
                                filterViewModel.Values.Any(
                                    v =>
                                    e.Skills != null
                                    && e.Skills.Any(
                                        s =>
                                            !string.IsNullOrWhiteSpace(s.Title)
                                            && s.Title.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0)));
                        break;
                }
            }

            return applicationUsersViewModel;
        }

        private void AddUserToMainWall(string userId)
        {
            var mainWall = _wallsDbSet
                .Include(x => x.Members)
                .FirstOrDefault(x => x.Type == WallType.Main && x.Members.All(m => m.UserId != userId));

            if (mainWall == null)
            {
                return;
            }

            var timestamps = DateTime.UtcNow;
            var userMainWall = new WallMember
            {
                UserId = userId,
                Wall = mainWall,
                CreatedBy = userId,
                ModifiedBy = userId,
                Created = timestamps,
                Modified = timestamps,
                AppNotificationsEnabled = true,
                EmailNotificationsEnabled = true
            };

            _wallUsersDbSet.Add(userMainWall);
        }
    }
}
