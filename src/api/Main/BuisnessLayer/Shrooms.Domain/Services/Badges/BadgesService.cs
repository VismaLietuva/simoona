using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataLayer.DAL;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Badges;
using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Badges
{
    public class BadgesService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<BadgeType> _badgeTypesDbSet;
        private readonly IDbSet<BadgeCategory> _badgeCategoriesDbSet;
        private readonly IDbSet<BadgeCategoryKudosType> _badgeCategoryKudosTypesDbSet;
        private readonly IDbSet<BadgeLog> _badgeLogsDbSet;

        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        public BadgesService(IUnitOfWork2 uow)
        {
            _uow = uow;

            _badgeTypesDbSet = _uow.GetDbSet<BadgeType>();
            _badgeCategoriesDbSet = _uow.GetDbSet<BadgeCategory>();
            _badgeCategoryKudosTypesDbSet = _uow.GetDbSet<BadgeCategoryKudosType>();
            _badgeLogsDbSet = _uow.GetDbSet<BadgeLog>();
            
            _usersDbSet = _uow.GetDbSet<ApplicationUser>();
            _kudosLogsDbSet = _uow.GetDbSet<KudosLog>();
        }

        #region Create
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

        public async Task AddBadgeCategoryAsync(string title,
                                                string description)
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
        #endregion

        #region Get
        public async Task<object> GetBadgeTypesByCategoryAsync(int badgeCategoryId)
            => await _badgeTypesDbSet.Where(x => x.BadgeCategoryId == badgeCategoryId).ToListAsync();

        public async Task<IList<BadgeCategory>> GetAllBadgeCategoriesAsync()
            => await _badgeCategoriesDbSet.ToListAsync();

        public async Task<object> GetAllUserBadgesAsync(string employeeId, int organizationId) 
            => await _badgeLogsDbSet.Where(x => x.OrganizationId == organizationId && x.EmployeeId == employeeId).ToListAsync();

        #endregion

        #region Update
        public async Task ActivateBadgeType(int badgeTypeId)
        {
            var badgeType = GetBadgeType(badgeTypeId);
            badgeType.IsActive = true;
            await SaveChangesAsync();
        }

        public async Task DeactivateBadgeType(int badgeTypeId)
        {
            var badgeType = GetBadgeType(badgeTypeId);
            badgeType.IsActive = false;
            await SaveChangesAsync();
        }
        #endregion

        #region Delete
        public async Task DeleteBadgeTypeAsync(int badgeTypeId)
        {
            var badgeType = GetBadgeType(badgeTypeId);
            _badgeTypesDbSet.Remove(badgeType);
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeCategoryAsync(int badgeCategoryId)
        {
            var badgeCategory = _badgeCategoriesDbSet.Find(badgeCategoryId) 
                                ?? throw new ValidationException(ErrorCodes.BadgeCategoryNotFound, $"Badge category with ID {badgeCategoryId} not found");
            _badgeCategoriesDbSet.Remove(badgeCategory);
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeCategoryFromKudosTypeAsync(int badgeCategoryId, int kudosTypeId)
        {
            var relationship = await _badgeCategoryKudosTypesDbSet
                                   .FirstOrDefaultAsync(x => x.BadgeCategoryId == badgeCategoryId && x.KudosTypeId == kudosTypeId)
                               ?? throw new ValidationException(ErrorCodes.BadgeToKudosRelationshipNotFound,
                                   $"Badge category (ID {badgeCategoryId}) and kudos type (ID {kudosTypeId}) relationship does not exist");

            _badgeCategoryKudosTypesDbSet.Remove(relationship);
            await SaveChangesAsync();
        }
        #endregion

        #region Other

        public async Task AssignBadgesAsync(int organizationId)
        {
            // TODO magic
            var categories = await GetAllBadgeCategoriesAsync();
            var badgeCategoryToKudosType = await _badgeCategoryKudosTypesDbSet.Include(x => x.KudosType).ToListAsync();
            var users = await _usersDbSet.Where(x => x.OrganizationId == organizationId).ToListAsync(); // Maybe in batches? In case we have tons of users, this might take a while

            foreach (var user in users.AsParallel())
            {
                var userId = user.Id;
                foreach (var category in categories.AsParallel())
                {
                    var categoryId = category.Id;
                    var relationshipWithKudos = badgeCategoryToKudosType.Where(x => x.BadgeCategoryId == categoryId);
                    var amount = 0;
                    foreach (var relationship in relationshipWithKudos)
                    {
                        var userKudos = await _kudosLogsDbSet.Where(x => x.EmployeeId == userId && x.Status == KudosStatus.Approved && !x.IsMinus()).ToListAsync();
                        switch (relationship.CalculationPolicyType)
                        {
                            case BadgeCalculationPolicyType.PointsStrategy:
                                amount = (int)Math.Round(userKudos.Sum(x => x.Points * x.MultiplyBy), 0);
                                break;
                            case BadgeCalculationPolicyType.MultiplierStrategy:
                                amount = userKudos.Sum(x => x.MultiplyBy);
                                break;
                        }
                    }

                    var availableKudosWithGivenAmount = await _badgeTypesDbSet.Where(x => x.IsActive && x.BadgeCategoryId == categoryId && x.Value <= amount).ToListAsync();
                    // TODO
                }
            }
        }

        #endregion

        #region Private methods
        private BadgeType GetBadgeType(int badgeTypeId) 
            => _badgeTypesDbSet.Find(badgeTypeId)
                    ?? throw new ValidationException(ErrorCodes.BadgeTypeNotFound, $"Badge type with ID {badgeTypeId} not found");
        
        private async Task SaveChangesAsync()
        {
            var response = await _uow.SaveChangesAsync();
            if (response == 0)
            {
                throw new ApplicationException("Changes could not be saved in BadgesService");
            }
        }
        #endregion
    }
}
