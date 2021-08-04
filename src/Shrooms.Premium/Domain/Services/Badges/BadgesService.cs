using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.Badges
{
    public class BadgesService : IBadgesService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly DbSet<BadgeType> _badgeTypesDbSet;
        private readonly DbSet<BadgeCategory> _badgeCategoriesDbSet;
        private readonly IDbSet<BadgeCategoryKudosType> _badgeCategoryKudosTypesDbSet;
        private readonly IDbSet<BadgeLog> _badgeLogsDbSet;

        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        public BadgesService(IUnitOfWork2 uow)
        {
            _uow = uow;

            _badgeTypesDbSet = _uow.GetDbSet<BadgeType>();
            _badgeCategoriesDbSet = _uow.GetDbSet<BadgeCategory>();
            _badgeCategoryKudosTypesDbSet = _uow.GetDbSet<BadgeCategoryKudosType>();
            _badgeLogsDbSet = _uow.GetDbSet<BadgeLog>();

            _kudosLogsDbSet = _uow.GetDbSet<KudosLog>();
            _usersDbSet = _uow.GetDbSet<ApplicationUser>();
        }

        public async Task AddBadgeTypeAsync(string title,
            string description,
            int value,
            string imageUrl,
            string imageSmallUrl,
            int badgeCategoryId)
        {
            var alreadyExists = await _badgeTypesDbSet
                .AnyAsync(t => t.Title == title);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, $"Badge type {title} already exists");
            }

            var entity = new BadgeType
            {
                Title = title,
                Description = description,
                Value = value,
                ImageUrl = imageUrl,
                ImageSmallUrl = imageSmallUrl,
                BadgeCategoryId = badgeCategoryId
            };

            _badgeTypesDbSet.Add(entity);

            await SaveChangesAsync();
        }

        public async Task AddBadgeCategoryAsync(string title, string description)
        {
            var alreadyExists = await _badgeCategoriesDbSet
                .AnyAsync(t => t.Title == title);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, $"Badge category {title} already exists");
            }

            var entity = new BadgeCategory
            {
                Title = title,
                Description = description
            };

            _badgeCategoriesDbSet.Add(entity);

            await SaveChangesAsync();
        }

        public async Task AddBadgeCategoryToKudosTypeAsync(int badgeCategoryId, int kudosTypeId, BadgeCalculationPolicyType calculationPolicy)
        {
            var alreadyExists = await _badgeCategoryKudosTypesDbSet
                .AnyAsync(x => x.BadgeCategoryId == badgeCategoryId && x.KudosTypeId == kudosTypeId);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable,
                    $"Badge category (ID {badgeCategoryId}) and kudos type (ID {kudosTypeId}) relationship already exists");
            }

            var entity = new BadgeCategoryKudosType
            {
                BadgeCategoryId = badgeCategoryId,
                KudosTypeId = kudosTypeId,
                CalculationPolicyType = calculationPolicy
            };

            _badgeCategoryKudosTypesDbSet.Add(entity);

            await SaveChangesAsync();
        }

        public async Task AddBadgeToUserAsync(int badgeTypeId, string employeeId, int organizationId)
        {
            var alreadyExists = await _badgeLogsDbSet
                .AnyAsync(x => x.BadgeTypeId == badgeTypeId && x.OrganizationId == organizationId && x.EmployeeId == employeeId);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable,
                    $"Badge type (ID {badgeTypeId}), employee (ID {employeeId}) and organization (ID {organizationId}) relationship already exists");
            }

            var entity = new BadgeLog
            {
                BadgeTypeId = badgeTypeId,
                EmployeeId = employeeId,
                OrganizationId = organizationId
            };

            _badgeLogsDbSet.Add(entity);

            await SaveChangesAsync();
        }

        public async Task<IList<BadgeCategory>> GetAllBadgeCategoriesAsync()
        {
            return await _badgeCategoriesDbSet
                .Include(x => x.RelationshipsWithKudosTypes.Select(y => y.KudosType))
                .Include(x => x.BadgeTypes)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ActivateBadgeTypeAsync(int badgeTypeId)
        {
            var badgeType = await GetBadgeTypeAsync(badgeTypeId);
            badgeType.IsActive = true;
            await SaveChangesAsync();
        }

        public async Task DeactivateBadgeTypeAsync(int badgeTypeId)
        {
            var badgeType = await GetBadgeTypeAsync(badgeTypeId);
            badgeType.IsActive = false;
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeTypeAsync(int badgeTypeId)
        {
            var badgeType = await GetBadgeTypeAsync(badgeTypeId);
            _badgeTypesDbSet.Remove(badgeType);
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeCategoryAsync(int badgeCategoryId)
        {
            var badgeCategory = await _badgeCategoriesDbSet.FindAsync(badgeCategoryId);

            if (badgeCategory == null)
            {
                throw new ValidationException(ErrorCodes.BadgeCategoryNotFound, $"Badge category with ID {badgeCategoryId} not found");
            }

            _badgeCategoriesDbSet.Remove(badgeCategory);
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeCategoryFromKudosTypeAsync(int badgeCategoryId, int kudosTypeId)
        {
            var relationship = await _badgeCategoryKudosTypesDbSet
                .FirstAsync(x => x.BadgeCategoryId == badgeCategoryId && x.KudosTypeId == kudosTypeId);

            _badgeCategoryKudosTypesDbSet.Remove(relationship);
            await SaveChangesAsync();
        }

        public async Task AssignBadgesAsync()
        {
            var categories = (await GetAllBadgeCategoriesAsync())
                .Where(x => x.RelationshipsWithKudosTypes.Any() &&
                            x.BadgeTypes.Any(y => y.IsActive))
                .ToList();

            if (!categories.Any())
            {
                return;
            }

            // TODO: with any open-source check-in add configuration key to set how many days ago we should check for changes here
            var oneDayAgo = DateTime.UtcNow.Date.AddDays(-1).Date;

            List<ApplicationUser> users;
            if (categories.Any(x => x.Created >= oneDayAgo || x.Modified >= oneDayAgo
                                                           || x.BadgeTypes.Any(y => y.IsActive && (y.Created >= oneDayAgo || y.Modified >= oneDayAgo))))
            {
                // We might have a new category or type, that means we have to take all users.
                users = await _usersDbSet.Where(x => x.TotalKudos > 0)
                    .Include(x => x.BadgeLogs.Select(y => y.BadgeType))
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                // We have no new badges, thus recalculate only for those that received new kudos
                users = await _kudosLogsDbSet.Where(x => x.Status == KudosStatus.Approved
                                                         && x.Modified > oneDayAgo
                                                         && !string.IsNullOrEmpty(x.EmployeeId)
                                                         && x.KudosSystemType != KudosTypeEnum.Minus)
                    .Select(x => x.Employee)
                    .Include(x => x.BadgeLogs.Select(y => y.BadgeType))
                    .Distinct(new EmployeeComparer())
                    .AsNoTracking()
                    .ToListAsync();
            }

            foreach (var user in users.AsParallel())
            {
                var userId = user.Id;
                var userKudosLogs = await _kudosLogsDbSet.Where(x => x.EmployeeId == userId
                                                                     && x.Status == KudosStatus.Approved
                                                                     && x.KudosSystemType != KudosTypeEnum.Minus)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var category in categories.AsParallel())
                {
                    var amount = 0;
                    foreach (var relationship in category.RelationshipsWithKudosTypes)
                    {
                        var kudosTypeName = relationship.KudosType.Name;
                        var userKudosLogsForBadgeCategory = userKudosLogs.Where(x => x.KudosTypeName == kudosTypeName);
                        switch (relationship.CalculationPolicyType)
                        {
                            case BadgeCalculationPolicyType.PointsStrategy:
                                amount += (int)Math.Round(userKudosLogsForBadgeCategory.Sum(x => x.Points), 0);
                                break;
                            case BadgeCalculationPolicyType.MultiplierStrategy:
                                amount += userKudosLogsForBadgeCategory.Sum(x => x.MultiplyBy);
                                break;
                        }
                    }

                    if (amount == 0)
                    {
                        continue;
                    }

                    var categoryId = category.Id;
                    var availableKudosWithGivenAmount = category.BadgeTypes.Where(x => x.IsActive && x.Value <= amount)
                        .ToList();
                    if (!availableKudosWithGivenAmount.Any())
                    {
                        continue;
                    }

                    var userAlreadyHasTheseKudosTypes = user.BadgeLogs.Where(x => x.EmployeeId == userId && x.BadgeType.BadgeCategoryId == categoryId)
                        .Select(x => x.BadgeTypeId)
                        .ToList();
                    if (availableKudosWithGivenAmount.Count == userAlreadyHasTheseKudosTypes.Count)
                    {
                        continue;
                    }

                    var organizationId = user.OrganizationId;
                    var badgesToAdd = availableKudosWithGivenAmount.Where(x => !userAlreadyHasTheseKudosTypes.Contains(x.Id));
                    foreach (var badgeToAdd in badgesToAdd)
                    {
                        await AddBadgeToUserAsync(badgeToAdd.Id, userId, organizationId);
                    }
                }
            }
        }

        private async Task<BadgeType> GetBadgeTypeAsync(int badgeTypeId)
        {
            var badgeType = await _badgeTypesDbSet.FindAsync(badgeTypeId);

            if (badgeType == null)
            {
                throw new ValidationException(ErrorCodes.BadgeTypeNotFound, $"Badge type with ID {badgeTypeId} not found");
            }

            return badgeType;
        }

        private async Task SaveChangesAsync()
        {
            var response = await _uow.SaveChangesAsync();
            if (response == 0)
            {
                throw new ApplicationException("Changes could not be saved in BadgesService");
            }
        }
    }
}
