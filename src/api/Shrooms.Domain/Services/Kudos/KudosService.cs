using Microsoft.EntityFrameworkCore;
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.ServiceValidators.Validators.Kudos;
using Shrooms.Resources;

namespace Shrooms.Domain.Services.Kudos
{
    public class KudosService : IKudosService
    {
        private const int LastPage = 1;

        private static readonly SemaphoreSlim _kudosLogLikeLock = new SemaphoreSlim(1, 1);

        private readonly IUnitOfWork2 _uow;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IKudosServiceValidator _kudosServiceValidator;
        private readonly IFilterPresetService _filterPresetService;
        private readonly IAsyncRunner _asyncRunner;
        private readonly DbSet<KudosLog> _kudosLogsDbSet;
        private readonly DbSet<KudosType> _kudosTypesDbSet;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly IRepository<KudosLog> _kudosLogRepository;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IApplicationSettings _settings;

        private Expression<Func<KudosType, bool>> _excludeNecessaryKudosTypes = x => x.Type != KudosTypeEnum.Send &&
                                                                                     x.Type != KudosTypeEnum.Minus &&
                                                                                     x.Type != KudosTypeEnum.Other &&
                                                                                     x.Type != KudosTypeEnum.Welcome &&
                                                                                     x.Type != KudosTypeEnum.Refund;

        private readonly ResourceManager _resourceManager;

        public KudosService(
            IUnitOfWork2 uow,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPermissionService permissionService,
            IKudosServiceValidator kudosServiceValidator,
            IAsyncRunner asyncRunner,
            IFilterPresetService filterPresetService,
            IApplicationSettings settings)
        {
            _uow = uow;
            _mapper = mapper;
            _permissionService = permissionService;
            _kudosServiceValidator = kudosServiceValidator;
            _asyncRunner = asyncRunner;
            _filterPresetService = filterPresetService;
            _settings = settings;
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _kudosLogRepository = unitOfWork.GetRepository<KudosLog>();
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();

            _resourceManager = new ResourceManager("Shrooms.Resources.Models.Kudos.Kudos", typeof(ResourceUtilities).Assembly);
        }

        public async Task CreateKudosTypeAsync(NewKudosTypeDto dto)
        {
            await ValidateKudosTypeNameIsUniqueAsync(dto.Name);

            var newType = new KudosType
            {
                Name = dto.Name,
                Value = dto.Multiplier,
                Type = KudosTypeEnum.Ordinary,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _kudosTypesDbSet.Add(newType);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task UpdateKudosTypeAsync(KudosTypeDto dto)
        {
            var type = await _kudosTypesDbSet
                .FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            if (type.Type == KudosTypeEnum.Ordinary)
            {
                await ValidateKudosTypeNameIsUniqueAsync(dto.Name, dto.Id);

                type.Name = dto.Name;
                type.Value = dto.Value;
                type.Description = dto.Description;
            }

            type.IsActive = dto.IsActive;

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task RemoveKudosTypeAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await _kudosTypesDbSet
                .Where(_excludeNecessaryKudosTypes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            var typeId = type.Id;

            _kudosTypesDbSet.Remove(type);

            await _uow.SaveChangesAsync(userOrg.UserId);

            await _filterPresetService.RemoveDeletedTypeFromPresetsAsync(typeId.ToString(), FilterType.Kudos, default);
        }

        public async Task<KudosTypeDto> GetSendKudosTypeAsync(UserAndOrganizationDto userOrg)
        {
            var sendType = (await _kudosTypesDbSet.Where(x => x.Type == KudosTypeEnum.Send).ToListAsync())
                .Select(MapKudosTypesToDto).FirstOrDefault();

            if (sendType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Types not found");
            }

            return sendType;
        }

        public async Task<KudosTypeDto> GetKudosTypeAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await _kudosTypesDbSet
                .Where(t => t.Id == id)
                .Select(t => new KudosTypeDto
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

        public async Task<KudosLogsEntriesDto<MainKudosLogDto>> GetKudosLogsAsync(KudosLogsFilterDto options)
        {
            var kudosLogsQueryBeforeSort = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log => log.OrganizationId == options.OrganizationId && log.KudosBasketId == null)
                .Where(KudosServiceHelper.StatusFilter(options.Status))
                .Where(KudosServiceHelper.UserFilter(options.SearchUserId))
                .Where(KudosServiceHelper.TypeFilter(options.FilteringType))
                .GroupJoin(_usersDbSet, log => log.CreatedBy, u => u.Id, KudosServiceHelper.MapKudosLogsToDto());

            var kudosLogsQuery = System.Linq.Dynamic.Core.DynamicQueryableExtensions.OrderBy(
                kudosLogsQueryBeforeSort.AsQueryable(), 
                string.Concat(options.SortBy, " ", options.SortOrder));

            var logsTotalCount = await kudosLogsQuery.CountAsync();

            var entriesCountToSkip = EntriesCountToSkip(options.Page);
            var kudosLogs = await kudosLogsQuery
                .Skip(entriesCountToSkip)
                .Take(BusinessLayerConstants.MaxKudosLogsPerPage)
                .ToListAsync();

            var user = await _usersDbSet.FindAsync(options.UserId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode ?? "en-US");

                foreach (var kudosLog in kudosLogs)
                {
                    if (IsTranslatableKudosType(kudosLog.Type.Type))
                    {
                        kudosLog.Type.Name = TranslateKudos("KudosType" + kudosLog.Type.Name, culture);
                    }
                }
            }

            return new KudosLogsEntriesDto<MainKudosLogDto>
            {
                KudosLogs = kudosLogs,
                TotalKudosCount = logsTotalCount
            };
        }

        public async Task<KudosLogsEntriesDto<KudosUserLogDto>> GetUserKudosLogsAsync(string userId, int page, int organizationId, IReadOnlyCollection<string> filteringTypes = null)
        {
            await ValidateUserAsync(organizationId, userId);

            var typeNames = filteringTypes?
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .ToArray();

            var filteredLogs = typeNames == null || typeNames.Length == 0
                ? _kudosLogsDbSet
                : _kudosLogsDbSet.Where(log => typeNames.Contains(log.KudosTypeName));

            var userLogsQuery = (from kudLog in filteredLogs
                                 where kudLog.EmployeeId == userId && kudLog.OrganizationId == organizationId
                                 from usr in _usersDbSet.Where(u => u.Id == kudLog.CreatedBy).DefaultIfEmpty()
                                 select new KudosUserLogDto
                                 {
                                     Comment = kudLog.Comments,
                                     Created = kudLog.Created,
                                     Id = kudLog.Id,
                                     Multiplier = kudLog.MultiplyBy,
                                     Points = kudLog.Points,
                                     Type = new KudosLogTypeDto
                                     {
                                         Name = kudLog.KudosTypeName,
                                         Value = kudLog.KudosTypeValue,
                                         Type = kudLog.KudosSystemType
                                     },
                                     Status = kudLog.Status.ToString(),
                                     Sender = new KudosLogUserDto
                                     {
                                         FullName = usr == null ? string.Empty : usr.FirstName + " " + usr.LastName,
                                         Id = usr == null ? string.Empty : kudLog.CreatedBy
                                     },
                                     PictureId = kudLog.PictureId
                                 }).OrderByDescending(o => o.Created).ThenByDescending(o => o.Id);

            var logCount = await userLogsQuery.CountAsync();

            var entriesCountToSkip = EntriesCountToSkip(page);
            var userLogs = await userLogsQuery
                .Skip(entriesCountToSkip)
                .Take(BusinessLayerConstants.MaxKudosLogsPerPage)
                .ToListAsync();

            var user = await _usersDbSet.FindAsync(userId);

            if (user != null)
            {
                var culture = CultureInfo.GetCultureInfo(user.CultureCode ?? "en-US");

                foreach (var userLog in userLogs)
                {
                    if (IsTranslatableKudosType(userLog.Type.Type))
                    {
                        userLog.Type.Name = TranslateKudos($"KudosType{userLog.Type.Name}", culture);
                    }
                }
            }

            return new KudosLogsEntriesDto<KudosUserLogDto>
            {
                KudosLogs = userLogs,
                TotalKudosCount = logCount
            };
        }

        public async Task<IEnumerable<WallKudosLogDto>> GetLastKudosLogsForWallAsync(UserAndOrganizationDto userAndOrg)
        {
            var approvedKudos = await _kudosLogsDbSet
                .Where(log =>
                    log.Status == KudosStatus.Approved &&
                    log.KudosSystemType != KudosTypeEnum.Minus &&
                    log.KudosSystemType != KudosTypeEnum.Refund &&
                    log.OrganizationId == userAndOrg.OrganizationId)
                .Join(_usersDbSet, l => l.CreatedBy, s => s.Id, (log, sender) => new { Log = log, Sender = sender, Receiver = log.Employee })
                .OrderByDescending(x => x.Log.Created)
                .Take(BusinessLayerConstants.WallKudosLogCount)
                .ToListAsync();

            var likerIds = approvedKudos
                .Where(x => x.Log.Likes != null)
                .SelectMany(x => x.Log.Likes.Select(like => like.UserId))
                .Distinct()
                .ToList();

            var likersById = likerIds.Count == 0
                ? new Dictionary<string, ApplicationUser>()
                : await _usersDbSet.Where(u => likerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            return approvedKudos
                .Select(x => new WallKudosLogDto
                {
                    Id = x.Log.Id,
                    Comment = x.Log.Comments,
                    Points = x.Log.Points,
                    Created = x.Log.Created,
                    PictureId = x.Log.PictureId,
                    Receiver = new KudosLogUserDto
                    {
                        FullName = x.Receiver != null ? x.Receiver.FirstName + " " + x.Receiver.LastName : null,
                        Id = x.Receiver != null ? x.Receiver.Id : null
                    },
                    Sender = new KudosLogUserDto
                    {
                        FullName = x.Sender.FirstName + " " + x.Sender.LastName,
                        Id = x.Log.KudosSystemType == KudosTypeEnum.Send ? x.Log.CreatedBy : null
                    },
                    Likes = MapLikesToDto(x.Log.Likes, likersById)
                })
                .ToList();
        }

        public async Task ToggleLikeAsync(AddLikeDto addLikeDto, UserAndOrganizationDto userOrg)
        {
            await _kudosLogLikeLock.WaitAsync();

            try
            {
                var kudosLog = await _kudosLogsDbSet
                    .FirstOrDefaultAsync(log =>
                        log.Id == addLikeDto.Id &&
                        log.OrganizationId == userOrg.OrganizationId &&
                        log.Status == KudosStatus.Approved);

                if (kudosLog == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Kudos log does not exist");
                }

                kudosLog.Likes ??= new LikesCollection();

                var like = kudosLog.Likes.FirstOrDefault(x => x.UserId == userOrg.UserId && x.Type == addLikeDto.Type);
                if (like == null)
                {
                    kudosLog.Likes.Add(new Like(userOrg.UserId, addLikeDto.Type));
                }
                else
                {
                    kudosLog.Likes.Remove(like);
                }

                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            finally
            {
                _kudosLogLikeLock.Release();
            }
        }

        private static IEnumerable<LikeDto> MapLikesToDto(LikesCollection likes, IReadOnlyDictionary<string, ApplicationUser> usersById)
        {
            if (likes == null)
            {
                return new List<LikeDto>();
            }

            return likes
                .Select(like => new { User = usersById.GetValueOrDefault(like.UserId), Like = like })
                .Where(likeWithUserData => likeWithUserData.User != null)
                .Select(likeWithUserData => new LikeDto
                {
                    UserId = likeWithUserData.User.Id,
                    FullName = likeWithUserData.User.FullName,
                    PictureId = likeWithUserData.User.PictureId,
                    Type = likeWithUserData.Like.Type
                })
                .ToList();
        }

        public async Task<IEnumerable<KudosPieChartSliceDto>> GetKudosPieChartDataAsync(int organizationId, string userId)
        {
            await ValidateUserAsync(organizationId, userId);

            var kudosLogs = await _kudosLogsDbSet
                .Where(kudos =>
                    kudos.EmployeeId == userId &&
                    kudos.Status == KudosStatus.Approved &&
                    kudos.KudosSystemType != KudosTypeEnum.Minus &&
                    kudos.OrganizationId == organizationId)
                .ToListAsync();

            var user = await _usersDbSet.FindAsync(userId);
            var culture = user == null ? null : CultureInfo.GetCultureInfo(user.CultureCode ?? "en-US");

            var pieChart = kudosLogs
                .GroupBy(log => log.KudosTypeName)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new KudosPieChartSliceDto
                {
                    TypeName = group.Key,
                    Name = culture != null && group.Any(log => IsTranslatableKudosType(log.KudosSystemType))
                        ? TranslateKudos($"KudosType{group.Key}", culture)
                        : group.Key,
                    Value = group.Sum(log => log.Points)
                })
                .ToList();

            return pieChart;
        }

        public async Task<IEnumerable<KudosTypeDto>> GetKudosTypesAsync(UserAndOrganizationDto userAndOrg)
        {
            var kudosTypes = (await _kudosTypesDbSet.ToListAsync()).Select(MapKudosTypesToDto).ToList();

            var user = await _usersDbSet.FindAsync(userAndOrg.UserId);

            if (user == null)
            {
                return kudosTypes;
            }

            var culture = CultureInfo.GetCultureInfo(user.CultureCode ?? "en-US");

            foreach (var kudosType in kudosTypes)
            {
                if (!IsTranslatableKudosType(kudosType.Type))
                {
                    continue;
                }

                kudosType.Name = TranslateKudos($"KudosType{kudosType.Name}", culture);
                kudosType.Description = TranslateKudos($"KudosType{kudosType.Name}Description", culture);
            }

            return kudosTypes;
        }

        public async Task<int> GetKudosTypeIdAsync(string kudosTypeName)
        {
            var kudosTypeId = await _kudosTypesDbSet
                .Where(t => t.Name == kudosTypeName)
                .Select(t => t.Id)
                .FirstAsync();

            return kudosTypeId;
        }

        public async Task<int> GetKudosTypeIdAsync(KudosTypeEnum kudosType)
        {
            return await _kudosTypesDbSet
                .Where(t => t.Type == kudosType)
                .Select(t => t.Id)
                .FirstAsync();
        }

        public async Task<IEnumerable<UserKudosInformationDto>> GetApprovedKudosListAsync(string id, int organizationId)
        {
            await ValidateUserAsync(organizationId, id);

            var kudosLogs = await _kudosLogsDbSet
                .Where(log =>
                    log.EmployeeId == id &&
                    log.Status == KudosStatus.Approved &&
                    log.OrganizationId == organizationId)
                .Select(MapKudosLogsToKudosInformationDto())
                .ToListAsync();

            return kudosLogs;
        }

        public async Task ApproveKudosAsync(int kudosLogId, UserAndOrganizationDto userOrg)
        {
            var kudosLog = await _kudosLogsDbSet
                .Include(x => x.Employee)
                .FirstAsync(x => x.Id == kudosLogId && x.OrganizationId == userOrg.OrganizationId);

            kudosLog.Approve(userOrg.UserId);

            if (!kudosLog.IsRecipientDeleted())
            {
                if (kudosLog.IsMinus())
                {
                    _asyncRunner.Run<IKudosNotificationService>(async notifier => await notifier.NotifyApprovedKudosDecreaseRecipientAsync(kudosLog), _uow.ConnectionName);
                }
                else
                {
                    _asyncRunner.Run<IKudosNotificationService>(async notifier => await notifier.NotifyApprovedKudosRecipientAsync(kudosLog), _uow.ConnectionName);
                }
            }

            await _uow.SaveChangesAsync(false);

            await UpdateProfileKudosAsync(kudosLog.Employee, userOrg);
        }

        public async Task RejectKudosAsync(KudosRejectDto kudosRejectDto)
        {
            var kudosLog = await _kudosLogsDbSet
                .Include(x => x.Employee)
                .FirstAsync(x => x.Id == kudosRejectDto.Id && x.OrganizationId == kudosRejectDto.OrganizationId);

            kudosLog.Reject(kudosRejectDto.UserId, kudosRejectDto.KudosRejectionMessage);

            if (!kudosLog.IsRecipientDeleted())
            {
                _asyncRunner.Run<IKudosNotificationService>(async notifier => await notifier.NotifyRejectedKudosLogSenderAsync(kudosLog), _uow.ConnectionName);
            }

            await _uow.SaveChangesAsync(false);
        }

        public async Task<UserKudosDto> GetUserKudosInformationByIdAsync(string id, int organizationId)
        {
            var user = await _usersDbSet
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Select(MapUserToKudosDto())
                .FirstAsync();

            return user;
        }

        public async Task<decimal[]> GetMonthlyKudosStatisticsAsync(string id)
        {
            if (id == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            var currentMonthLogs = await _kudosLogRepository
                .Get(l => l.CreatedBy == id &&
                          l.Created.Month == now.Month &&
                          l.Created.Year == now.Year &&
                          l.KudosSystemType == KudosTypeEnum.Send)
                .ToListAsync();

            var sentThisMonth = currentMonthLogs.Sum(log => log.Points);
            var remaining = (await _applicationUserRepository.GetByIdAsync(id)).RemainingKudos;
            var maxAvailableToSend = _settings.KudosAvailableToSendPerMonth ?? BusinessLayerConstants.DefaultKudosAvailableToSendPerMonth;

            var availableToSendThisMonth = Math.Max(0, Math.Min(remaining, maxAvailableToSend - sentThisMonth));

            return new[] { sentThisMonth, availableToSendThisMonth };
        }

        public async Task AddKudosLogAsync(AddKudosLogDto kudosDto, decimal? points = null)
        {
            if (!await UserHasPermissionAsync(kudosDto))
            {
                throw new ValidationException(ErrorCodes.KudosTypeNotFound);
            }

            await AddKudosRequestAsync(kudosDto, points);
        }

        private async Task<bool> UserHasPermissionAsync(AddKudosLogDto kudosDto)
        {
            var kudosType = _kudosTypesDbSet.AsNoTracking().FirstOrDefault(p => p.Id == kudosDto.PointsTypeId);
            if (kudosType is null)
            {
                return false;
            }

            if (kudosType.IsActive || kudosType.Type == KudosTypeEnum.Send)
            {
                return true;
            }

            return await HasKudosAdministratorPermissionAsync(kudosDto);
        }

        public async Task AddRefundKudosLogsAsync(IEnumerable<AddKudosLogDto> kudosLogs)
        {
            foreach (var log in kudosLogs)
            {
                var kudosDto = await MapInitialInfoToDtoAsync(log);

                var applicationUser = await _usersDbSet.FindAsync(log.ReceivingUserIds.First());
                kudosDto.ReceivingUser = _mapper.Map<ApplicationUserDto>(applicationUser);

                InsertKudosLog(kudosDto, KudosStatus.Approved);
            }
        }

        private async Task AddKudosRequestAsync(AddKudosLogDto kudosLog, decimal? points = null)
        {
            var kudosDto = await MapInitialInfoToDtoAsync(kudosLog, points);

            var receivingUsers = await _usersDbSet
                .Where(x => kudosLog.ReceivingUserIds.Contains(x.Id) && x.OrganizationId == kudosLog.OrganizationId)
                .ToListAsync();

            var sendingUser = await _usersDbSet
                .FirstOrDefaultAsync(x => x.Id == kudosDto.SendingUser.Id && x.OrganizationId == kudosLog.OrganizationId);

            _kudosServiceValidator.CheckForEmptyUserList(receivingUsers);

            foreach (var receivingUser in receivingUsers)
            {
                _kudosServiceValidator.ValidateUser(receivingUser);
                kudosDto.ReceivingUser = _mapper.Map<ApplicationUserDto>(receivingUser);
                await ChooseKudosifyTypeAsync(kudosDto);
            }

            await _uow.SaveChangesAsync(false);

            if (kudosDto.KudosType.Type != KudosTypeEnum.Send)
            {
                return;
            }

            foreach (var receivingUser in receivingUsers)
            {
                _asyncRunner.Run<IKudosNotificationService>(async notifier => await notifier.NotifyAboutKudosSentAsync(kudosDto), _uow.ConnectionName);
                await UpdateProfileKudosAsync(receivingUser, kudosLog);
            }

            await UpdateProfileKudosAsync(sendingUser, kudosLog);
        }

        public async Task<IEnumerable<KudosBasicDataDto>> GetKudosStatsAsync(int months, int amount, int organizationId)
        {
            var date = DateTime.UtcNow.AddMonths(-months);

            var kudosLogsStats = await _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log => log.OrganizationId == organizationId
                              && log.KudosBasketId == null
                              && log.Status == KudosStatus.Approved
                              && log.KudosSystemType != KudosTypeEnum.Minus
                              && log.Created >= date
                              && log.Employee != null)
                .GroupBy(log => log.Employee.Id)
                .Select(log => new KudosBasicDataDto
                {
                    Name = log.Key,
                    KudosAmount = log.Sum(s => s.Points)
                })
                .OrderByDescending(log => log.KudosAmount)
                .Take(amount)
                .ToListAsync();

            var userIds = kudosLogsStats.Select(s => s.Name).ToArray();

            var users = await _usersDbSet
                .Where(w => userIds.Contains(w.Id))
                .ToListAsync();

            kudosLogsStats.ForEach(f => f.Name = users.Single(s => s.Id == f.Name).FullName);

            return kudosLogsStats;
        }

        public async Task UpdateProfileKudosAsync(ApplicationUser user, UserAndOrganizationDto userOrg)
        {
            await SetKudosToUserProfileAsync(user, userOrg);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task UpdateProfilesFromUserIdsAsync(IEnumerable<string> usersId, UserAndOrganizationDto userOrg)
        {
            foreach (var id in usersId)
            {
                var user = await _usersDbSet.FindAsync(id);
                await SetKudosToUserProfileAsync(user, userOrg);
            }
        }

        private async Task SetKudosToUserProfileAsync(ApplicationUser user, UserAndOrganizationDto userOrg)
        {
            var allUserKudosLogs = _kudosLogsDbSet
                .Where(x =>
                    x.EmployeeId == user.Id &&
                    x.Status == KudosStatus.Approved &&
                    x.Created >= user.EmploymentDate &&
                    x.OrganizationId == userOrg.OrganizationId);

            var kudosTotal = await allUserKudosLogs
                .Where(x => x.KudosSystemType != KudosTypeEnum.Minus &&
                            x.KudosSystemType != KudosTypeEnum.Refund &&
                            x.KudosBasketId == null)
                .SumAsync(x => (decimal?)x.Points);

            user.TotalKudos = kudosTotal ?? 0;

            var spentKudos = await allUserKudosLogs
                .Where(x => x.KudosSystemType == KudosTypeEnum.Minus ||
                            x.KudosBasketId != null)
                .SumAsync(x => (decimal?)x.Points);

            var refundedKudos = await allUserKudosLogs
                .Where(x => x.KudosSystemType == KudosTypeEnum.Refund &&
                            x.KudosBasketId == null)
                .SumAsync(x => (decimal?)x.Points) ?? 0;

            spentKudos -= refundedKudos;

            user.SpentKudos = spentKudos ?? 0;
            user.RemainingKudos = user.TotalKudos - user.SpentKudos;
        }

        public async Task<bool> HasPendingKudosAsync(string employeeId)
        {
            IList<KudosLog> kudosLogs = await _kudosLogsDbSet.Where(e => e.EmployeeId == employeeId && e.Status == KudosStatus.Pending).ToListAsync();

            return kudosLogs.Any();
        }

        private static Expression<Func<ApplicationUser, UserKudosDto>> MapUserToKudosDto()
        {
            return x => new UserKudosDto
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                PictureId = x.PictureId,
                RemainingKudos = x.RemainingKudos,
                SpentKudos = x.SpentKudos,
                TotalKudos = x.TotalKudos
            };
        }

        private async Task ValidateUserAsync(int organizationId, string userId)
        {
            var userExists = await _usersDbSet.AnyAsync(x => x.Id == userId && x.OrganizationId == organizationId);

            _kudosServiceValidator.CheckIfUserExists(userExists);
        }

        private Expression<Func<KudosLog, UserKudosInformationDto>> MapKudosLogsToKudosInformationDto()
        {
            return x => new UserKudosInformationDto
            {
                Comments = x.Comments,
                Created = x.Created,
                MultiplyBy = x.MultiplyBy,
                Points = x.Points,
                Type = new KudosTypeDto
                {
                    Name = x.KudosTypeName,
                    Value = x.KudosTypeValue
                },
                Sender = _mapper.Map<ApplicationUserDto>(_usersDbSet.FirstOrDefault(u => u.Id == x.CreatedBy))
            };
        }

        private static int EntriesCountToSkip(int pageRequested)
        {
            return (pageRequested - LastPage) * BusinessLayerConstants.MaxKudosLogsPerPage;
        }

        private async Task ValidateKudosTypeNameIsUniqueAsync(string name, int? excludedTypeId = null)
        {
            var lowerCaseName = name?.ToLower();

            var typesWithSameName = _kudosTypesDbSet.Where(type => type.Name.ToLower() == lowerCaseName);

            if (excludedTypeId.HasValue)
            {
                typesWithSameName = typesWithSameName.Where(type => type.Id != excludedTypeId.Value);
            }

            if (await typesWithSameName.AnyAsync())
            {
                throw new ValidationException(ErrorCodes.KudosTypeNameAlreadyExists, "Kudos type name should be unique");
            }
        }

        private static KudosTypeDto MapKudosTypesToDto(KudosType kudosType)
        {
            return new KudosTypeDto
            {
                Id = kudosType.Id,
                Name = kudosType.Name,
                Value = kudosType.Value,
                Type = kudosType.Type,
                Description = kudosType.Description,
                IsNecessary = kudosType.Type == KudosTypeEnum.Send ||
                              kudosType.Type == KudosTypeEnum.Minus ||
                              kudosType.Type == KudosTypeEnum.Other,
                IsActive = kudosType.IsActive
            };
        }

        private async Task<AddKudosDto> MapInitialInfoToDtoAsync(AddKudosLogDto kudosLog, decimal? overridenPoints = null)
        {
            var sendingUser = await _usersDbSet.FindAsync(kudosLog.UserId);
            _kudosServiceValidator.ValidateUser(sendingUser);

            var kudosType = await _kudosTypesDbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == kudosLog.PointsTypeId);
            _kudosServiceValidator.ValidateKudosType(kudosType);

            return new AddKudosDto
            {
                KudosLog = kudosLog,
                KudosType = MapKudosTypesToDto(kudosType),
                SendingUser = _mapper.Map<ApplicationUserDto>(sendingUser),
                TotalKudosPointsInLog = overridenPoints ?? kudosLog.MultiplyBy * kudosType?.Value ?? 0,
                PictureId = kudosLog.PictureId
            };
        }

        private async Task ChooseKudosifyTypeAsync(AddKudosDto kudos)
        {
            switch (kudos.KudosType.Type)
            {
                case KudosTypeEnum.Minus:
                    await MinusKudosAsync(kudos);
                    break;

                case KudosTypeEnum.Send:
                    await SendKudosAsync(kudos);
                    break;

                default:
                    AddNewKudos(kudos);
                    break;
            }
        }

        public async Task AddLotteryKudosLogAsync(AddKudosLogDto kudosLogDto, UserAndOrganizationDto userOrg)
        {
            var kudosDto = await MapInitialInfoToDtoAsync(kudosLogDto);

            var receivingUser = await _usersDbSet
                .FirstOrDefaultAsync(x => x.Id == kudosLogDto.UserId && x.OrganizationId == kudosLogDto.OrganizationId);

            if (receivingUser == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "User not found");
            }

            kudosDto.ReceivingUser = _mapper.Map<ApplicationUserDto>(receivingUser);

            InsertKudosLog(kudosDto, KudosStatus.Approved);

            await _uow.SaveChangesAsync(false);

            await UpdateProfileKudosAsync(_mapper.Map<ApplicationUser>(kudosDto.SendingUser), userOrg);
        }

        private async Task MinusKudosAsync(AddKudosDto kudos)
        {
            var hasKudosAdminPermission = await HasKudosAdministratorPermissionAsync(kudos.KudosLog);
            var hasKudosServiceRequestCategoryPermission = await HasKudosServiceRequestCategoryPermissionsAsync();
            _kudosServiceValidator.ValidateKudosMinusPermission(hasKudosAdminPermission || hasKudosServiceRequestCategoryPermission);

            InsertKudosLog(kudos, KudosStatus.Pending);
        }

        private async Task SendKudosAsync(AddKudosDto kudos)
        {
            await ValidateSendKudosAsync(kudos);
            await InsertSendKudosLogAsync(kudos);
            kudos.TotalPointsSent += kudos.TotalKudosPointsInLog;
        }

        private async Task ValidateSendKudosAsync(AddKudosDto kudos)
        {
            _kudosServiceValidator.ValidateSendingToSameUserAsReceiving(kudos.SendingUser.Id, kudos.ReceivingUser.Id);

            _kudosServiceValidator.ValidateUserAvailableKudos(kudos.SendingUser.RemainingKudos, kudos.TotalKudosPointsInLog * kudos.KudosLog.ReceivingUserIds.Count());

            await ValidateAvailableKudosThisMonthAsync(kudos, kudos.TotalKudosPointsInLog);
        }

        private async Task InsertSendKudosLogAsync(AddKudosDto kudos)
        {
            InsertKudosLog(kudos, KudosStatus.Approved);

            var minusKudos = await GenerateLogForKudosMinusOperationAsync(kudos);

            InsertKudosLog(minusKudos, KudosStatus.Approved);
        }

        private async Task<AddKudosDto> GenerateLogForKudosMinusOperationAsync(AddKudosDto kudos)
        {
            var minusKudosType = await _kudosTypesDbSet.AsNoTracking().FirstAsync(n => n.Type == KudosTypeEnum.Minus);

            var kudosLogForMinusKudos = new AddKudosLogDto
            {
                PointsTypeId = minusKudosType.Id,
                Comment = kudos.KudosLog.Comment,
                MultiplyBy = kudos.KudosLog.MultiplyBy,
                OrganizationId = kudos.KudosLog.OrganizationId,
                UserId = kudos.KudosLog.UserId
            };

            var minusKudos = new AddKudosDto
            {
                KudosLog = kudosLogForMinusKudos,
                ReceivingUser = kudos.SendingUser,
                SendingUser = kudos.ReceivingUser,
                TotalKudosPointsInLog = kudos.TotalKudosPointsInLog,
                KudosType = MapKudosTypesToDto(minusKudosType),
                PictureId = kudos.PictureId
            };

            return minusKudos;
        }

        private void AddNewKudos(AddKudosDto kudos)
        {
            InsertKudosLog(kudos, KudosStatus.Pending);
        }

        private async Task ValidateAvailableKudosThisMonthAsync(AddKudosDto kudos, decimal totalKudosPointsInLog)
        {
            var timestamp = DateTime.UtcNow;

            var currentMonthSum = await _kudosLogsDbSet
                .Where(l => l.CreatedBy == kudos.SendingUser.Id &&
                            l.Created.Month == timestamp.Month &&
                            l.Created.Year == timestamp.Year &&
                            l.KudosSystemType == KudosTypeEnum.Send &&
                            l.OrganizationId == kudos.KudosLog.OrganizationId)
                .SumAsync(p => (decimal?)p.Points) ?? 0m;

            currentMonthSum += kudos.TotalPointsSent;

            var availableToSendPerMonth = _settings.KudosAvailableToSendPerMonth ?? BusinessLayerConstants.DefaultKudosAvailableToSendPerMonth;
            _kudosServiceValidator.ValidateUserAvailableKudosToSendPerMonth(totalKudosPointsInLog, availableToSendPerMonth - currentMonthSum);
        }

        private void InsertKudosLog(AddKudosDto kudos, KudosStatus status)
        {
            var log = new KudosLog
            {
                CreatedBy = kudos.KudosLog.UserId,
                EmployeeId = kudos.ReceivingUser.Id,
                ModifiedBy = kudos.KudosLog.UserId,
                MultiplyBy = kudos.KudosLog.MultiplyBy,
                KudosTypeName = kudos.KudosType.Name,
                KudosTypeValue = kudos.KudosType.Value,
                KudosSystemType = kudos.KudosType.Type,
                Status = status,
                Comments = kudos.KudosLog.Comment,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                OrganizationId = kudos.KudosLog.OrganizationId,
                Points = kudos.TotalKudosPointsInLog,
                PictureId = kudos.PictureId
            };

            _kudosLogsDbSet.Add(log);
        }

        private async Task<bool> HasKudosAdministratorPermissionAsync(UserAndOrganizationDto userAndOrg)
        {
            return await _permissionService.UserHasPermissionAsync(userAndOrg, AdministrationPermissions.Kudos);
        }

        private async Task<bool> HasKudosServiceRequestCategoryPermissionsAsync()
        {
            ApplicationUser kudosServiceRequestCategory;

            try
            {
                kudosServiceRequestCategory = await _usersDbSet
                    .Include(x => x.ServiceRequestCategoriesAssigned)
                    .FirstOrDefaultAsync(x => x.ServiceRequestCategoriesAssigned.Any(y => y.Name == "Kudos"));
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

        public async Task<WelcomeKudosDto> GetWelcomeKudosAsync()
        {
            var welcomeKudos = await _kudosTypesDbSet
                .Where(kudosType => kudosType.Type == KudosTypeEnum.Welcome)
                .Select(kudosType => new WelcomeKudosDto
                {
                    WelcomeKudosAmount = kudosType.Value,
                    WelcomeKudosComment = kudosType.Description,
                    WelcomeKudosIsActive = kudosType.IsActive
                })
                .FirstOrDefaultAsync();

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

        private static bool IsTranslatableKudosType(KudosTypeEnum type)
        {
            return type == KudosTypeEnum.Send ||
                   type == KudosTypeEnum.Minus ||
                   type == KudosTypeEnum.Other;
        }
    }
}
