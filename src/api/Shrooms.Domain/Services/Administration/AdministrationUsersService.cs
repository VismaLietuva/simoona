using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Administration;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Services.Email.AdministrationUsers;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.ServiceValidators.Validators.UserAdministration;
using Shrooms.Infrastructure.ExcelGenerator;
using UserResources = Shrooms.Resources.Models.ApplicationUser.ApplicationUser;

namespace Shrooms.Domain.Services.Administration
{
    public class AdministrationUsersService : IAdministrationUsersService
    {
        private const string UsersExcelWorksheetName = "Users";

        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<ApplicationRole> _rolesRepository;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IDbSet<DataLayer.EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly IDbSet<WallMember> _wallUsersDbSet;
        private readonly ShroomsUserManager _userManager;
        private readonly IUserAdministrationValidator _userAdministrationValidator;
        private readonly IOrganizationService _organizationService;
        private readonly IPictureService _pictureService;
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAdministrationNotificationService _notificationService;
        private readonly IKudosService _kudosService;
        private readonly IUnitOfWork2 _uow;
        private readonly IExcelBuilderFactory _excelBuilderFactory;

        public AdministrationUsersService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IUnitOfWork2 uow,
            IUserAdministrationValidator userAdministrationValidator,
            ShroomsUserManager userManager,
            IOrganizationService organizationService,
            IPictureService pictureService,
            IDbContext context,
            IAdministrationNotificationService notificationService,
            IKudosService kudosService,
            IExcelBuilderFactory excelBuilderFactory)
        {
            _uow = uow;
            _mapper = mapper;
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _rolesRepository = unitOfWork.GetRepository<ApplicationRole>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
            _wallsDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _wallUsersDbSet = uow.GetDbSet<WallMember>();
            _userManager = userManager;
            _userAdministrationValidator = userAdministrationValidator;
            _organizationService = organizationService;
            _pictureService = pictureService;
            _context = context;
            _notificationService = notificationService;
            _kudosService = kudosService;
            _excelBuilderFactory = excelBuilderFactory;
        }

        public async Task<ByteArrayContent> GetAllUsersExcelAsync(string fileName, int organizationId)
        {
            var users = await _usersDbSet
                .Include(user => user.WorkingHours)
                .Include(user => user.JobPosition)
                .Where(user => user.OrganizationId == organizationId)
                .ToListAsync();

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(UsersExcelWorksheetName)
                .AddHeader(
                    UserResources.FirstName,
                    UserResources.LastName,
                    UserResources.Birthday,
                    UserResources.JobTitle,
                    UserResources.PhoneNumber,
                    UserResources.WorkingHours,
                    UserResources.HasPicture)
                .AddRows(users.AsQueryable(), MapUserToExcelRow());

            var content = new ByteArrayContent(excelBuilder.Build());

            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"{fileName}.xlsx"
            };

            return content;
        }

        public async Task<bool> UserIsSoftDeletedAsync(string email)
        {
            if (_context is not ShroomsDbContext shroomsContext)
            {
                throw new ArgumentNullException(nameof(shroomsContext));
            }

            var user = await shroomsContext
                .Users
                .SqlQuery("SELECT * FROM [dbo].[AspNetUsers] WHERE Email = @email", new SqlParameter("@email", email))
                .SingleOrDefaultAsync();

            return user != null;
        }

        public async Task RestoreUserAsync(string email)
        {
            var shroomsContext = _context as ShroomsDbContext;

            if (shroomsContext == null)
            {
                throw new ArgumentNullException(nameof(shroomsContext));
            }

            await shroomsContext.Database
                .ExecuteSqlCommandAsync("UPDATE [dbo].[AspNetUsers] SET[IsDeleted] = '0' WHERE Email = @email", new SqlParameter("@email", email));

            var user = await _userManager.FindByEmailAsync(email);
            await AddNewUserRolesAsync(user.Id);
        }

        public async Task AddProviderImageAsync(string userId, ClaimsIdentity externalIdentity)
        {
            var user = await _usersDbSet.FirstAsync(u => u.Id == userId);
            if (user.PictureId == null && externalIdentity.FindFirst("picture") != null)
            {
                byte[] data = data = await new WebClient().DownloadDataTaskAsync(externalIdentity.FindFirst("picture").Value);
                user.PictureId = await _pictureService.UploadFromStreamAsync(new MemoryStream(data), "image/jpeg", Guid.NewGuid() + ".jpg", user.OrganizationId);
                await _uow.SaveChangesAsync(userId);
            }
        }

        public async Task NotifyAboutNewUserAsync(ApplicationUser user, int orgId)
        {
            await _notificationService.NotifyAboutNewUserAsync(user, orgId);
        }

        public async Task SendUserPasswordResetEmailAsync(ApplicationUser user, string organizationName)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);

            await _notificationService.SendUserResetPasswordEmailAsync(user, token, organizationName);
        }

        public async Task SendUserVerificationEmailAsync(ApplicationUser user, string orgazinationName)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);

            await _notificationService.SendUserVerificationEmailAsync(user, token, orgazinationName);
        }

        public async Task ConfirmNewUserAsync(string userId, UserAndOrganizationDto userAndOrg)
        {
            var applicationUser = await _usersDbSet.FirstAsync(user => user.Id == userId);
            _userAdministrationValidator.CheckIfEmploymentDateIsSet(applicationUser.EmploymentDate);

            var hasRole = await _userManager.IsInRoleAsync(userId, Contracts.Constants.Roles.FirstLogin);
            _userAdministrationValidator.CheckIfUserHasFirstLoginRole(hasRole);

            var addRoleResult = await _userManager.AddToRoleAsync(userId, Contracts.Constants.Roles.User);
            var removeRoleResult = await _userManager.RemoveFromRoleAsync(userId, Contracts.Constants.Roles.NewUser);

            _userAdministrationValidator.CheckForAddingRemovingRoleErrors(addRoleResult.Errors.ToList(), removeRoleResult.Errors.ToList());
            await _notificationService.SendConfirmedNotificationEmailAsync(applicationUser.Email, userAndOrg);

            SetTutorialStatus(applicationUser, false);

            await SetWelcomeKudosAsync(applicationUser);

            await AddWallsToNewUser(applicationUser, userAndOrg);
            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        public async Task<IdentityResult> CreateNewUserWithExternalLoginAsync(ExternalLoginInfo info, string requestedOrganization)
        {
            var externalIdentity = info.ExternalIdentity;
            var userSettings = await _organizationDbSet.Where(o => o.ShortName == requestedOrganization)
                .Select(u => new { u.CultureCode, u.TimeZone })
                .FirstAsync();

            var user = new ApplicationUser
            {
                UserName = externalIdentity.FindFirst(ClaimTypes.Email).Value,
                Email = externalIdentity.FindFirst(ClaimTypes.Email).Value,
                FirstName = externalIdentity.FindFirst(ClaimTypes.GivenName).Value,
                LastName = externalIdentity.FindFirst(ClaimTypes.Surname).Value,
                OrganizationId = (await _organizationService.GetOrganizationByNameAsync(requestedOrganization)).Id,
                EmploymentDate = DateTime.UtcNow,
                CultureCode = userSettings.CultureCode ?? BusinessLayerConstants.DefaultCulture,
                TimeZone = userSettings.TimeZone,
                NotificationsSettings = null
            };

            if (externalIdentity.FindFirst("picture") != null)
            {
                var data = await new WebClient().DownloadDataTaskAsync(externalIdentity.FindFirst("picture").Value);
                var picture = await _pictureService.UploadFromStreamAsync(new MemoryStream(data), "image/jpeg", $"{Guid.NewGuid()}.jpg", user.OrganizationId);
                user.PictureId = picture;
            }

            var result = _userManager.Create(user);
            if (!result.Succeeded)
            {
                return result;
            }

            await AddNewUserRolesAsync(user.Id);
            return result;
        }

        public async Task<IdentityResult> CreateNewUserAsync(ApplicationUser user, string password, string requestedOrganization)
        {
            var userSettings = await _organizationDbSet.Where(o => o.ShortName == requestedOrganization)
                .Select(u => new { u.CultureCode, u.TimeZone })
                .FirstAsync();

            user.OrganizationId = (await _organizationService.GetOrganizationByNameAsync(requestedOrganization)).Id;
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

            await AddNewUserRolesAsync(user.Id);
            await SendUserVerificationEmailAsync(user, requestedOrganization);

            return result;
        }

        public async Task<bool> HasExistingExternalLoginAsync(string email, string loginProvider)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var hasLogin = user.Logins.Any(login => login.LoginProvider == loginProvider);
            return hasLogin;
        }

        public async Task<bool> UserEmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task<IEnumerable<AdministrationUserDto>> GetAllUsersAsync(string sortQuery, string search, FilterDto[] filterModel, string includeProperties)
        {
            includeProperties = $"{includeProperties}{(includeProperties != string.Empty ? "," : string.Empty)}Roles,Skills,JobPosition,Projects";

            var applicationUsers = await _applicationUserRepository
                .Get(GenerateQuery(search), orderBy: sortQuery.Contains(Contracts.Constants.Roles.NewUser) ? string.Empty : sortQuery, includeProperties: includeProperties)
                .ToListAsync();

            var administrationUsers = _mapper.Map<IList<ApplicationUser>, IList<AdministrationUserDto>>(applicationUsers);

            await SetNewUsersValuesAsync(administrationUsers, applicationUsers);

            if (filterModel != null)
            {
                administrationUsers = FilterResults(filterModel, administrationUsers);
            }

            if (sortQuery.StartsWith(Contracts.Constants.Roles.NewUser))
            {
                administrationUsers = (sortQuery.EndsWith("asc") ? administrationUsers.OrderBy(u => u.IsNewUser) : administrationUsers.OrderByDescending(u => u.IsNewUser)).ToList();
            }

            return administrationUsers;
        }

        public async Task SetUserTutorialStatusToCompleteAsync(string userId)
        {
            var applicationUser = await _usersDbSet.FirstAsync(user => user.Id == userId);
            SetTutorialStatus(applicationUser, true);
            await _uow.SaveChangesAsync(userId);
        }

        public async Task<bool> GetUserTutorialStatusAsync(string userId)
        {
            return (await _usersDbSet.FirstAsync(user => user.Id == userId)).IsTutorialComplete;
        }

        public async Task AddProviderEmailAsync(string userId, string provider, string email)
        {
            var user = await _usersDbSet.FirstAsync(u => u.Id == userId);
            if (provider == AuthenticationConstants.GoogleLoginProvider)
            {
                user.GoogleEmail = email;
            }

            if (provider == AuthenticationConstants.FacebookLoginProvider)
            {
                user.FacebookEmail = email;
            }
            if (provider == AuthenticationConstants.MicrosoftLoginProvider)
            {
                user.MicrosoftEmail = email;
            }

            await _uow.SaveChangesAsync(userId);
        }

        private static Expression<Func<ApplicationUser, IExcelRow>> MapUserToExcelRow()
        {
            return user => new ExcelRow
            {
                new ExcelColumn
                {
                    Value = user.FirstName
                },

                new ExcelColumn
                {
                    Value = user.LastName
                },

                new ExcelColumn
                {
                    Value = user.BirthDay,
                    Format = ExcelWorksheetBuilderConstants.DateWithoutTimeFormat
                },

                new ExcelColumn
                {
                    Value = user.JobPosition == null ? string.Empty : user.JobPosition.Title
                },

                new ExcelColumn
                {
                    Value = StringToLong(user.PhoneNumber),
                    Format = ExcelWorksheetBuilderConstants.NumberFormat
                },

                new ExcelColumn
                {
                    Value = user.WorkingHours != null ? $"{user.WorkingHours.StartTime}-{user.WorkingHours.EndTime}" : string.Empty,
                    SetHorizontalTextCenter = true
                },

                new ExcelColumn
                {
                    Value = user.PictureId != null ? Resources.Common.Yes : Resources.Common.No,
                    SetHorizontalTextCenter = true
                }
            };
        }

        private async Task SetWelcomeKudosAsync(ApplicationUser applicationUser)
        {
            var welcomeKudosDto = await _kudosService.GetWelcomeKudosAsync();

            if (welcomeKudosDto.WelcomeKudosAmount <= 0)
            {
                return;
            }

            var welcomeKudos = new KudosLog
            {
                EmployeeId = applicationUser.Id,
                OrganizationId = applicationUser.OrganizationId,
                Comments = welcomeKudosDto.WelcomeKudosComment,
                Points = welcomeKudosDto.WelcomeKudosAmount,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Status = KudosStatus.Pending,
                MultiplyBy = 1,
                KudosSystemType = KudosTypeEnum.Welcome,
                KudosTypeValue = Convert.ToDecimal(KudosTypeEnum.Welcome),
                KudosTypeName = KudosTypeEnum.Welcome.ToString()
            };

            _uow.GetDbSet<KudosLog>().Add(welcomeKudos);
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

        private async Task AddNewUserRolesAsync(string id)
        {
            await _userManager.AddToRoleAsync(id, Contracts.Constants.Roles.NewUser);
            await _userManager.AddToRoleAsync(id, Contracts.Constants.Roles.FirstLogin);
        }

        private async Task SetNewUsersValuesAsync(IList<AdministrationUserDto> administrationUserDto, IEnumerable<ApplicationUser> applicationUsers)
        {
            var newUserRole = await _rolesRepository.Get(x => x.Name == Contracts.Constants.Roles.NewUser).Select(x => x.Id).FirstOrDefaultAsync();

            var usersWaitingForConfirmationIds =
                applicationUsers.Where(x => x.Roles.Any(y => y.RoleId == newUserRole)).Select(x => x.Id).ToList();

            foreach (var user in usersWaitingForConfirmationIds)
            {
                administrationUserDto.First(x => x.Id == user).IsNewUser = true;
            }
        }

        private static IList<AdministrationUserDto> FilterResults(IEnumerable<FilterDto> filterModel, IList<AdministrationUserDto> administrationUsers)
        {
            IEnumerable<AdministrationUserDto> filteredUsers = null;

            foreach (var filterViewModel in filterModel)
            {
                switch (filterViewModel.Key.ToLowerInvariant())
                {
                    case "jobtitle":
                        filteredUsers = administrationUsers
                            .Where(e =>
                                filterViewModel.Values.Any(v =>
                                    !string.IsNullOrWhiteSpace(e.JobTitle)
                                    && e.JobTitle.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0));
                        break;

                    case "projects":
                        filteredUsers = administrationUsers
                            .Where(e =>
                                filterViewModel.Values.Any(v =>
                                    e.Projects != null
                                    && e.Projects.Any(p =>
                                        !string.IsNullOrWhiteSpace(p.Name)
                                        && (p.Name.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0))));
                        break;

                    case "skills":
                        filteredUsers = administrationUsers
                            .Where(e =>
                                filterViewModel.Values.Any(v =>
                                    e.Skills != null
                                    && e.Skills.Any(s =>
                                        !string.IsNullOrWhiteSpace(s.Title)
                                        && s.Title.IndexOf(v, StringComparison.OrdinalIgnoreCase) >= 0)));
                        break;
                }
            }

            return filteredUsers?.ToList() ?? administrationUsers;
        }

        private async Task<IList<DataLayer.EntityModels.Models.Multiwall.Wall>> GetWallsToAddForNewUserAsync(ApplicationUser applicationUser)
        {
            return await _wallsDbSet
                .Include(wall => wall.Members)
                .Where(wall => wall.OrganizationId == applicationUser.OrganizationId &&
                    (wall.Type == WallType.Main || (wall.AddForNewUsers && wall.Type == WallType.UserCreated)) &&
                    wall.Members.All(m => m.UserId != applicationUser.Id))
                .ToListAsync();
        }

        private async Task AddWallsToNewUser(ApplicationUser applicationUser, UserAndOrganizationDto userOrg)
        {
            var walls = await GetWallsToAddForNewUserAsync(applicationUser);

            if (!walls.Any())
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            foreach (var wall in walls)
            {
                var wallMember = new WallMember
                {
                    UserId = applicationUser.Id,
                    Wall = wall,
                    CreatedBy = userOrg.UserId,
                    ModifiedBy = userOrg.UserId,
                    Created = timestamp,
                    Modified = timestamp,
                    AppNotificationsEnabled = true,
                    EmailNotificationsEnabled = true
                };

                _wallUsersDbSet.Add(wallMember);
            }
        }
    }
}
