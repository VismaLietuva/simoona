using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Resources;
using System.Threading.Tasks;
using DomainServiceValidators.Validators.Kudos;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Resources;

namespace Shrooms.Domain.Services.Kudos
{
    public class KudosService : IKudosService
    {
        private const int LastPage = 1;

        private readonly IUnitOfWork2 _uow;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IKudosServiceValidator _kudosServiceValidator;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IRepository<KudosLog> _kudosLogRepository;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private Expression<Func<KudosType, bool>> _excludeNecessaryKudosTypes = x => x.Type != ConstBusinessLayer.KudosTypeEnum.Send &&
                              x.Type != ConstBusinessLayer.KudosTypeEnum.Minus &&
                              x.Type != ConstBusinessLayer.KudosTypeEnum.Other;

        private readonly ResourceManager _resourceManager;

        public KudosService(
            IUnitOfWork2 uow,
            IUnitOfWork unitOfWork,
            IRoleService roleService,
            IPermissionService permissionService,
            IKudosServiceValidator kudosServiceValidator,
            IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _roleService = roleService;
            _permissionService = permissionService;
            _kudosServiceValidator = kudosServiceValidator;
            _asyncRunner = asyncRunner;
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _kudosLogRepository = unitOfWork.GetRepository<KudosLog>();
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();

            _resourceManager = new ResourceManager("Shrooms.Resources.Models.Kudos.Kudos", typeof(ResourceUtilities).Assembly);
        }

        public async Task CreateKudosType(NewKudosTypeDto dto)
        {
            var alreadyExists = await _kudosTypesDbSet
                .AnyAsync(t => t.Name == dto.Name);

            if (alreadyExists)
            {
                throw new ValidationException(444, "Kudos type already exists");
            }

            var newType = new KudosType
            {
                Name = dto.Name,
                Value = dto.Multiplier,
                Type = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _kudosTypesDbSet.Add(newType);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task UpdateKudosType(KudosTypeDTO dto)
        {
            var type = await _kudosTypesDbSet
                .Where(_excludeNecessaryKudosTypes)
                .FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            type.Name = dto.Name;
            type.Value = dto.Value;
            type.Description = dto.Description;
            type.IsActive = dto.IsActive;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task RemoveKudosType(int id, UserAndOrganizationDTO userOrg)
        {
            var type = await _kudosTypesDbSet
                .Where(_excludeNecessaryKudosTypes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            _kudosTypesDbSet.Remove(type);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<KudosTypeDTO> GetKudosType(int id, UserAndOrganizationDTO userOrg)
        {
            var type = await _kudosTypesDbSet
                .Where(t => t.Id == id)
                .Where(_excludeNecessaryKudosTypes)
                .Select(t => new KudosTypeDTO
                {
                    Id = t.Id,
                    Name = t.Name,
                    Value = t.Value,
                    Description = t.Description,
                    IsActive = t.IsActive,
                    Type = t.Type
                })
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            return type;
        }

        public KudosLogsEntriesDto<MainKudosLogDTO> GetKudosLogs(KudosLogsFilterDTO options)
        {
            var kudosLogsQuery = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log =>
                    log.OrganizationId == options.OrganizationId &&
                    log.KudosBasketId == null)
                .Where(KudosServiceHelper.StatusFilter(options.Status))
                .Where(KudosServiceHelper.UserFilter(options.SearchUserId))
                .Where(KudosServiceHelper.TypeFilter(options.FilteringType))
                .GroupJoin(_usersDbSet, log => log.CreatedBy, u => u.Id, KudosServiceHelper.MapKudosLogsToDto())
                .OrderBy(string.Concat(options.SortBy, " ", options.SortOrder));

            var logsTotalCount = kudosLogsQuery.Count();

            var entriesCountToSkip = EntriesCountToSkip(options.Page);
            var kudosLogs = kudosLogsQuery
                .Skip(() => entriesCountToSkip)
                .Take(() => ConstBusinessLayer.MaxKudosLogsPerPage)
                .ToList();

            var user = _usersDbSet.Find(options.UserId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode);

                foreach (var kudosLog in kudosLogs)
                {
                    if (IsTranslatableKudosType(kudosLog.Type.Type))
                    {
                        kudosLog.Type.Name = TranslateKudos("KudosType" + kudosLog.Type.Name, culture);
                    }
                }
            }

            return new KudosLogsEntriesDto<MainKudosLogDTO>
            {
                KudosLogs = kudosLogs,
                TotalKudosCount = logsTotalCount
            };
        }

        public KudosLogsEntriesDto<KudosUserLogDTO> GetUserKudosLogs(string userId, int page, int organizationId)
        {
            ValidateUser(organizationId, userId);

            var userLogsQuery = (from kudLog in _kudosLogsDbSet
                                 where kudLog.EmployeeId == userId && kudLog.OrganizationId == organizationId
                                 from usr in _usersDbSet.Where(u => u.Id == kudLog.CreatedBy).DefaultIfEmpty()
                                 select new KudosUserLogDTO
                                 {
                                     Comment = kudLog.Comments,
                                     Created = kudLog.Created,
                                     Id = kudLog.Id,
                                     Multiplier = kudLog.MultiplyBy,
                                     Points = kudLog.Points,
                                     Type = new KudosLogTypeDTO
                                     {
                                         Name = kudLog.KudosTypeName,
                                         Value = kudLog.KudosTypeValue,
                                         Type = kudLog.KudosSystemType
                                     },
                                     Status = kudLog.Status.ToString(),
                                     Sender = new KudosLogUserDTO
                                     {
                                         FullName = usr == null ? string.Empty : usr.FirstName + " " + usr.LastName,
                                         Id = usr == null ? string.Empty : kudLog.CreatedBy
                                     },
                                     PictureId = kudLog.PictureId
                                 }).OrderByDescending(o => o.Created);

            var logCount = userLogsQuery.Count();

            var entriesCountToSkip = EntriesCountToSkip(page);
            var userLogs = userLogsQuery
                .Skip(() => entriesCountToSkip)
                .Take(() => ConstBusinessLayer.MaxKudosLogsPerPage)
                .ToList();
            var user = _usersDbSet.Find(userId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode);

                foreach (var userLog in userLogs)
                {
                    if (IsTranslatableKudosType(userLog.Type.Type))
                    {
                        userLog.Type.Name = TranslateKudos($"KudosType{userLog.Type.Name}", culture);
                    }
                }
            }

            return new KudosLogsEntriesDto<KudosUserLogDTO>
            {
                KudosLogs = userLogs,
                TotalKudosCount = logCount
            };
        }

        public IEnumerable<WallKudosLogDTO> GetLastKudosLogsForWall(UserAndOrganizationDTO userAndOrg)
        {
            var approvedKudos = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log =>
                    log.Status == KudosStatus.Approved &&
                    log.KudosSystemType != ConstBusinessLayer.KudosTypeEnum.Minus &&
                    log.OrganizationId == userAndOrg.OrganizationId)
                .Join(_usersDbSet,
                    l => l.CreatedBy,
                    s => s.Id,
                    MapKudosLogToWallKudosLogDTO())
                .OrderByDescending(log => log.Created)
                .Take(() => ConstBusinessLayer.WallKudosLogCount)
                .ToList();

            return approvedKudos;
        }

        public IEnumerable<KudosPieChartSliceDto> GetKudosPieChartData(int organizationId, string userId)
        {
            ValidateUser(organizationId, userId);

            var kudosLogs =
                _kudosLogsDbSet
                    .Where(kudos =>
                        kudos.EmployeeId == userId &&
                        kudos.Status == KudosStatus.Approved &&
                        kudos.KudosSystemType != ConstBusinessLayer.KudosTypeEnum.Minus &&
                        kudos.OrganizationId == organizationId)
                    .ToList();

            var user = _usersDbSet.Find(userId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode);

                foreach (var kudosLog in kudosLogs)
                {
                    if (IsTranslatableKudosType(kudosLog.KudosSystemType))
                    {
                        kudosLog.KudosTypeName = TranslateKudos($"KudosType{kudosLog.KudosTypeName}", culture);
                    }
                }
            }

            var pieChart = kudosLogs
                .GroupBy(log => log.KudosTypeName)
                .Select(group => new KudosPieChartSliceDto
                {
                    Name = group.Key,
                    Value = group.Sum(log => log.Points)
                });

            return pieChart;
        }

        public IEnumerable<KudosTypeDTO> GetKudosTypes(UserAndOrganizationDTO userAndOrg)
        {
            var hasKudosAdminPermission = HasKudosAdministratorPermission(userAndOrg);

            var kudosTypesDTO = _kudosTypesDbSet
                .Where(GetKudosTypeQuery(hasKudosAdminPermission))
                .Select(MapKudosTypesToDTO)
                .ToList();

            var user = _usersDbSet.Find(userAndOrg.UserId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode);

                foreach (var kudosType in kudosTypesDTO)
                {
                    if (IsTranslatableKudosType(kudosType.Type))
                    {
                        kudosType.Name = TranslateKudos($"KudosType{kudosType.Name}", culture);
                        kudosType.Description = TranslateKudos($"KudosType{kudosType.Name}Description", culture);
                    }
                }
            }

            return kudosTypesDTO;
        }

        public int GetKudosTypeId(string kudosTypeName)
        {
            var kudosTypeId = _kudosTypesDbSet
                .Where(t => t.Name == kudosTypeName)
                .Select(t => t.Id)
                .First();

            return kudosTypeId;
        }

        public IEnumerable<UserKudosInformationDTO> GetApprovedKudosList(string id, int organizationId)
        {
            ValidateUser(organizationId, id);

            var kudosLogs = _kudosLogsDbSet
                .Where(log =>
                    log.EmployeeId == id &&
                    log.Status == KudosStatus.Approved &&
                    log.OrganizationId == organizationId)
                .Select(MapKudosLogsToKudosInformationDTO())
                .ToList();

            return kudosLogs;
        }

        public IEnumerable<UserKudosAutocompleteDTO> GetUsersForAutocomplete(string s)
        {
            var users = _usersDbSet
                .Where(user => user.UserName.Contains(s) || user.Email.Contains(s) || (user.FirstName + " " + user.LastName).Contains(s))
                .Where(_roleService.ExcludeUsersWithRole(Constants.Authorization.Roles.NewUser))
                .Select(MapUsersToAutocompleteDTO())
                .ToList();

            return users;
        }

        public void ApproveKudos(int kudosLogId, UserAndOrganizationDTO userOrg)
        {
            var kudosLog = _kudosLogsDbSet
                .Include(x => x.Employee)
                .First(x =>
                    x.Id == kudosLogId &&
                    x.OrganizationId == userOrg.OrganizationId);

            kudosLog.Approve(userOrg.UserId);

            if (!kudosLog.IsRecipientDeleted())
            {
                if (kudosLog.IsMinus())
                {
                    _asyncRunner.Run<IKudosNotificationService>(n => n.NotifyApprovedKudosDecreaseRecipient(kudosLog), _uow.ConnectionName);
                }
                else
                {
                    _asyncRunner.Run<IKudosNotificationService>(n => n.NotifyApprovedKudosRecipient(kudosLog), _uow.ConnectionName);
                }
            }

            _uow.SaveChanges(false);

            UpdateProfileKudos(kudosLog.Employee, userOrg);
        }

        public void RejectKudos(KudosRejectDTO kudosRejectDto)
        {
            var kudosLog = _kudosLogsDbSet
                .Include(x => x.Employee)
                .First(x =>
                    x.Id == kudosRejectDto.id &&
                    x.OrganizationId == kudosRejectDto.OrganizationId);

            kudosLog.Reject(kudosRejectDto.UserId, kudosRejectDto.kudosRejectionMessage);

            if (!kudosLog.IsRecipientDeleted())
            {
                _asyncRunner.Run<IKudosNotificationService>(n => n.NotifyRejectedKudosLogSender(kudosLog), _uow.ConnectionName);
            }

            _uow.SaveChanges(false);
        }

        public UserKudosDTO GetUserKudosInformationById(string id, int organizationId)
        {
            var user = _usersDbSet
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Select(MapUserToKudosDTO())
                .First();

            return user;
        }

        public decimal[] GetMonthlyKudosStatistics(string id)
        {
            if (id == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            decimal available = 0;

            var currentMonthLogs = _kudosLogRepository
                .Get(l => l.CreatedBy == id &&
                    l.Created.Month == now.Month &&
                    l.Created.Year == now.Year &&
                    l.KudosSystemType == ConstBusinessLayer.KudosTypeEnum.Send)
                .ToList();

            var sentThisMonth = currentMonthLogs.Sum(log => log.Points);
            var remaining = _applicationUserRepository.GetByID(id).RemainingKudos;

            if (sentThisMonth < ConstBusinessLayer.KudosAvailableToSendThisMonth)
            {
                available = ConstBusinessLayer.KudosAvailableToSendThisMonth - sentThisMonth < remaining ?
                    ConstBusinessLayer.KudosAvailableToSendThisMonth - sentThisMonth : remaining;
            }

            return new[] { sentThisMonth, available < 0 ? 0 : available };
        }

        public void AddKudosLog(AddKudosLogDTO kudosLog)
        {
            if (UserHasPermission(kudosLog))
            {
                AddKudosRequest(kudosLog);
            }
        }

        public void AddKudosLog(AddKudosLogDTO kudosDto, decimal points)
        {
            if (UserHasPermission(kudosDto))
            {
                AddKudosRequest(kudosDto, points);
            }
        }

        private bool UserHasPermission(AddKudosLogDTO kudosDto)
        {
            if (kudosDto.IsActive)
            {
                return true;
            }

            var authorizedRoles = new List<string>() { Constants.Authorization.Roles.Admin, Constants.Authorization.Roles.KudosAdmin };

            if (authorizedRoles.Any(role => _roleService.HasRole(kudosDto.UserId, role)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddKudosRequest(AddKudosLogDTO kudosLog, decimal? points = null)
        {
            var kudosDto = MapInitialInfoToDTO(kudosLog, points);

            var receivingUsers = _usersDbSet
                .Where(x => kudosLog.ReceivingUserIds.Contains(x.Id) &&
                            x.OrganizationId == kudosLog.OrganizationId)
                .ToList();

            _kudosServiceValidator.CheckForEmptyUserList(receivingUsers);

            foreach (var receivingUser in receivingUsers)
            {
                _kudosServiceValidator.ValidateUser(receivingUser);
                kudosDto.ReceivingUser = receivingUser;
                ChooseKudosifyType(kudosDto);
            }

            _uow.SaveChanges(false);

            foreach (var receivingUser in receivingUsers)
            {
                if (kudosDto.KudosType.Type == ConstBusinessLayer.KudosTypeEnum.Send)
                {
                    kudosDto.ReceivingUser = receivingUser;
                    _asyncRunner.Run<IKudosNotificationService>(n => n.NotifyAboutKudosSent(kudosDto), _uow.ConnectionName);
                    UpdateProfileKudos(kudosDto.ReceivingUser, kudosLog);
                }
            }

            if (kudosDto.KudosType.Type == ConstBusinessLayer.KudosTypeEnum.Send)
            {
                UpdateProfileKudos(kudosDto.SendingUser, kudosLog);
            }
        }

        public IEnumerable<KudosBasicDataDTO> GetKudosStats(int months, int amount, int organizationId)
        {
            var date = DateTime.UtcNow.AddMonths(-months);

            var kudosLogsStats = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log => log.OrganizationId == organizationId
                    && log.KudosBasketId == null
                    && log.Status == KudosStatus.Approved
                    && log.KudosSystemType != ConstBusinessLayer.KudosTypeEnum.Minus
                    && log.Created >= date
                    && log.Employee != null)
                .GroupBy(log => log.Employee.Id)
                .Select(log => new KudosBasicDataDTO
                {
                    Name = log.Key,
                    KudosAmount = log.Sum(s => s.Points)
                })
                .OrderByDescending(log => log.KudosAmount)
                .Take(() => amount)
                .ToList();

            var userIds = kudosLogsStats.Select(s => s.Name).ToArray();

            var users = _usersDbSet
                .Where(w => userIds.Contains(w.Id))
                .ToList();

            kudosLogsStats.ForEach(f => f.Name = users.Single(s => s.Id == f.Name).FullName);

            return kudosLogsStats;
        }

        public void UpdateProfileKudos(ApplicationUser user, UserAndOrganizationDTO userOrg)
        {
            var allUserKudosLogs = _kudosLogsDbSet
                .Where(x => x.EmployeeId == user.Id &&
                            x.Status == KudosStatus.Approved &&
                            x.Created >= user.EmploymentDate &&
                            x.OrganizationId == userOrg.OrganizationId);

            var kudosTotal = allUserKudosLogs
                .Where(x => x.KudosSystemType != ConstBusinessLayer.KudosTypeEnum.Minus &&
                            x.KudosBasketId == null)
                .Sum(x => (decimal?)x.Points);

            user.TotalKudos = kudosTotal ?? 0;

            var spentKudos = allUserKudosLogs
                .Where(x => x.KudosSystemType == ConstBusinessLayer.KudosTypeEnum.Minus ||
                            x.KudosBasketId != null)
                .Sum(x => (decimal?)x.Points);

            user.SpentKudos = spentKudos ?? 0;
            user.RemainingKudos = user.TotalKudos - user.SpentKudos;
            _uow.SaveChanges(userOrg.UserId);
        }

        public bool HasPendingKudos(string employeeId)
        {
            IList<KudosLog> kudosLogs = _kudosLogsDbSet.Where(e => e.EmployeeId == employeeId &&
                                                                    e.Status == KudosStatus.Pending).ToList();

            return kudosLogs.Any();
        }

        private static Expression<Func<ApplicationUser, UserKudosAutocompleteDTO>> MapUsersToAutocompleteDTO()
        {
            return u => new UserKudosAutocompleteDTO
            {
                Id = u.Id,
                FormattedName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                UserName = u.UserName,
                PictureId = u.PictureId
            };
        }

        private static Expression<Func<ApplicationUser, UserKudosDTO>> MapUserToKudosDTO()
        {
            return x => new UserKudosDTO
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                PictureId = x.PictureId,
                RemainingKudos = x.RemainingKudos,
                SpentKudos = x.SpentKudos,
                TotalKudos = x.TotalKudos
            };
        }

        private void ValidateUser(int organizationId, string userId)
        {
            var userExists = _usersDbSet.Any(x => x.Id == userId && x.OrganizationId == organizationId);

            _kudosServiceValidator.CheckIfUserExists(userExists);
        }

        private Expression<Func<KudosLog, UserKudosInformationDTO>> MapKudosLogsToKudosInformationDTO()
        {
            return x => new UserKudosInformationDTO
            {
                Comments = x.Comments,
                Created = x.Created,
                MultiplyBy = x.MultiplyBy,
                Points = x.Points,
                Type = new KudosTypeDTO
                {
                    Name = x.KudosTypeName,
                    Value = x.KudosTypeValue
                },
                Sender = _usersDbSet.Find(x.CreatedBy)
            };
        }

        private static int EntriesCountToSkip(int pageRequested)
        {
            return (pageRequested - LastPage) * ConstBusinessLayer.MaxKudosLogsPerPage;
        }

        private KudosTypeDTO MapKudosTypesToDTO(KudosType kudosType)
        {
            return new KudosTypeDTO
            {
                Id = kudosType.Id,
                Name = kudosType.Name,
                Value = kudosType.Value,
                Type = kudosType.Type,
                Description = kudosType.Description,
                IsNecessary = kudosType.Type == ConstBusinessLayer.KudosTypeEnum.Send ||
                              kudosType.Type == ConstBusinessLayer.KudosTypeEnum.Minus ||
                              kudosType.Type == ConstBusinessLayer.KudosTypeEnum.Other,
                IsActive = kudosType.IsActive
            };
        }

        private Expression<Func<KudosType, bool>> GetKudosTypeQuery(bool hasKudosAdminPermission)
        {
            if (hasKudosAdminPermission)
            {
                return x => true;
            }

            return type => type.Type != ConstBusinessLayer.KudosTypeEnum.Minus;
        }

        private Expression<Func<KudosLog, ApplicationUser, WallKudosLogDTO>> MapKudosLogToWallKudosLogDTO()
        {
            return (log, sender) => new WallKudosLogDTO
            {
                Comment = log.Comments,
                Points = log.Points,
                Created = log.Created,
                PictureId = log.PictureId,
                Receiver = new KudosLogUserDTO
                {
                    FullName = log.Employee != null ? log.Employee.FirstName + " " + log.Employee.LastName : null,
                    Id = log.Employee != null ? log.Employee.Id : null
                },
                Sender = new KudosLogUserDTO
                {
                    FullName = sender.FirstName + " " + sender.LastName,
                    Id = log.KudosSystemType == ConstBusinessLayer.KudosTypeEnum.Send ? log.CreatedBy : null
                }
            };
        }

        private AddKudosDTO MapInitialInfoToDTO(AddKudosLogDTO kudosLog, decimal? overridenPoints = null)
        {
            var sendingUser = _usersDbSet.Find(kudosLog.UserId);
            _kudosServiceValidator.ValidateUser(sendingUser);

            var kudosType = _kudosTypesDbSet.Find(kudosLog.PointsTypeId);
            _kudosServiceValidator.ValidateKudosType(kudosType);

            return new AddKudosDTO
            {
                KudosLog = kudosLog,
                KudosType = kudosType,
                SendingUser = sendingUser,
                TotalKudosPointsInLog = overridenPoints ?? kudosLog.MultiplyBy * kudosType.Value,
                PictureId = kudosLog.PictureId
            };
        }

        private void ChooseKudosifyType(AddKudosDTO kudosDTO)
        {
            switch (kudosDTO.KudosType.Type)
            {
                case ConstBusinessLayer.KudosTypeEnum.Minus:
                    MinusKudos(kudosDTO);
                    break;

                case ConstBusinessLayer.KudosTypeEnum.Send:
                    SendKudos(kudosDTO);
                    break;

                default:
                    AddNewKudos(kudosDTO);
                    break;
            }
        }

        private void MinusKudos(AddKudosDTO kudosDTO)
        {
            var hasKudosAdminPermission = HasKudosAdministratorPermission(kudosDTO.KudosLog);
            var hasKudosServiceRequestCategoryPermission = HasKudosServiceRequestCategoryPermissions();
            _kudosServiceValidator.ValidateKudosMinusPermission(hasKudosAdminPermission || hasKudosServiceRequestCategoryPermission);

            InsertKudosLog(kudosDTO, KudosStatus.Pending);
        }

        private void SendKudos(AddKudosDTO kudosDTO)
        {
            ValidateSendKudos(kudosDTO);
            InsertSendKudosLog(kudosDTO);
            kudosDTO.TotalPointsSent += kudosDTO.TotalKudosPointsInLog;
        }

        private void ValidateSendKudos(AddKudosDTO kudosDTO)
        {
            _kudosServiceValidator.ValidateSendingToSameUserAsReceiving(
                kudosDTO.SendingUser.Id,
                kudosDTO.ReceivingUser.Id);

            _kudosServiceValidator.ValidateUserAvailableKudos(
                kudosDTO.SendingUser.RemainingKudos,
                kudosDTO.TotalKudosPointsInLog * kudosDTO.KudosLog.ReceivingUserIds.Count());

            ValidateAvailableKudosThisMonth(kudosDTO, kudosDTO.TotalKudosPointsInLog);
        }

        private void InsertSendKudosLog(AddKudosDTO kudosDTO)
        {
            InsertKudosLog(kudosDTO, KudosStatus.Approved);

            var minusKudosDTO = GenerateLogForKudosMinusOperation(kudosDTO);

            InsertKudosLog(minusKudosDTO, KudosStatus.Approved);
        }

        private AddKudosDTO GenerateLogForKudosMinusOperation(AddKudosDTO kudosDTO)
        {
            var minusKudosType = _kudosTypesDbSet
                    .FirstOrDefault(n => n.Type == ConstBusinessLayer.KudosTypeEnum.Minus);

            var kudosLogForMinusKudos = new AddKudosLogDTO
            {
                PointsTypeId = minusKudosType.Id,
                Comment = kudosDTO.KudosLog.Comment,
                MultiplyBy = kudosDTO.KudosLog.MultiplyBy,
                OrganizationId = kudosDTO.KudosLog.OrganizationId,
                UserId = kudosDTO.KudosLog.UserId
            };

            var minusKudosDTO = new AddKudosDTO
            {
                KudosLog = kudosLogForMinusKudos,
                ReceivingUser = kudosDTO.SendingUser,
                SendingUser = kudosDTO.ReceivingUser,
                TotalKudosPointsInLog = kudosDTO.TotalKudosPointsInLog,
                KudosType = minusKudosType,
                PictureId = kudosDTO.PictureId
            };

            return minusKudosDTO;
        }

        private void AddNewKudos(AddKudosDTO kudosDTO)
        {
            InsertKudosLog(kudosDTO, KudosStatus.Pending);
        }

        private void ValidateAvailableKudosThisMonth(AddKudosDTO kudosDTO, decimal totalKudosPointsInLog)
        {
            var timestaps = DateTime.UtcNow;

            var currentMonthSum = _kudosLogsDbSet
                            .Where(l => l.CreatedBy == kudosDTO.SendingUser.Id &&
                                l.Created.Month == timestaps.Month &&
                                l.Created.Year == timestaps.Year &&
                                l.KudosSystemType == ConstBusinessLayer.KudosTypeEnum.Send &&
                                l.OrganizationId == kudosDTO.KudosLog.OrganizationId)
                            .Select(p => p.Points)
                            .DefaultIfEmpty(0)
                            .Sum();

            currentMonthSum += kudosDTO.TotalPointsSent;

            _kudosServiceValidator.ValidateUserAvailableKudosToSendPerMonth(totalKudosPointsInLog, ConstBusinessLayer.KudosAvailableToSendThisMonth - currentMonthSum);
        }

        private void InsertKudosLog(AddKudosDTO kudosDTO, KudosStatus status)
        {
            var log = new KudosLog
            {
                CreatedBy = kudosDTO.KudosLog.UserId,
                EmployeeId = kudosDTO.ReceivingUser.Id,
                ModifiedBy = kudosDTO.KudosLog.UserId,
                MultiplyBy = kudosDTO.KudosLog.MultiplyBy,
                KudosTypeName = kudosDTO.KudosType.Name,
                KudosTypeValue = kudosDTO.KudosType.Value,
                KudosSystemType = kudosDTO.KudosType.Type,
                Status = status,
                Comments = kudosDTO.KudosLog.Comment,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                OrganizationId = kudosDTO.KudosLog.OrganizationId,
                Points = kudosDTO.TotalKudosPointsInLog,
                PictureId = kudosDTO.PictureId
            };
            _kudosLogsDbSet.Add(log);
        }

        private bool HasKudosAdministratorPermission(UserAndOrganizationDTO userAndOrg)
        {
            return _permissionService.UserHasPermission(userAndOrg, AdministrationPermissions.Kudos);
        }

        private bool HasKudosServiceRequestCategoryPermissions()
        {
            ApplicationUser kudosServiceRequestCategory;

            try
            {
                kudosServiceRequestCategory = _usersDbSet
                    .Include(x => x.ServiceRequestCategoriesAssigned)
                    .FirstOrDefault(x => x.ServiceRequestCategoriesAssigned.Any(y => y.Name == "Kudos"));
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            if (kudosServiceRequestCategory == null)
            {
                return false;
            }

            return true;
        }

        public WelcomeKudosDTO GetWelcomeKudos()
        {
            var welcomeKudos = _kudosTypesDbSet
                .Where(kudosType => kudosType.Type == ConstBusinessLayer.KudosTypeEnum.Welcome)
                .Select(kudosType => new WelcomeKudosDTO()
                {
                    KudosWelcomeAmount = kudosType.Value,
                    KudosWelcomeComment = kudosType.Description,
                    KudosWelcomeIsActive = kudosType.IsActive
                })
                .FirstOrDefault();

            if (welcomeKudos == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Welcome kudos not found");
            }

            return welcomeKudos;
        }

        private string TranslateKudos(string textToTranslate, CultureInfo culture)
        {
            return ResourceUtilities.GetResourceValue(_resourceManager, textToTranslate, culture);
        }

        private static bool IsTranslatableKudosType(ConstBusinessLayer.KudosTypeEnum type)
        {
            return type == ConstBusinessLayer.KudosTypeEnum.Send ||
                   type == ConstBusinessLayer.KudosTypeEnum.Minus ||
                   type == ConstBusinessLayer.KudosTypeEnum.Other;
        }
    }
}