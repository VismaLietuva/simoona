using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.DataLayer.DAL;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Badges;

namespace Shrooms.Domain.Services.Badges
{
    public class BadgesService
    {
        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<BadgeType> _badgeTypesDbSet;
        private readonly IDbSet<BadgeCategory> _badgeCategoriesDbSet;
        private readonly IDbSet<BadgeCategoryKudosType> _badgeCategoryKudosTypesDbSet;
        private readonly IDbSet<BadgeLog> _badgeLogsDbSet;
        public BadgesService(IUnitOfWork2 uow)
        {
            _uow = uow;

            _badgeTypesDbSet = _uow.GetDbSet<BadgeType>();
            _badgeCategoriesDbSet = _uow.GetDbSet<BadgeCategory>();
            _badgeCategoryKudosTypesDbSet = _uow.GetDbSet<BadgeCategoryKudosType>();
            _badgeLogsDbSet = _uow.GetDbSet<BadgeLog>();
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
                throw new ValidationException(444, $"Badge type {title} already exists");
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
                throw new ValidationException(444, $"Badge category {title} already exists");
            }

            var entity = new BadgeCategory
            {
                Title = title,
                Description = description
            };

            _badgeCategoriesDbSet.Add(entity);

            await SaveChangesAsync();
        }

        public async Task AddBadgeCategoryToKudosTypeAsync(int badgeCategoryId, int kudosTypeId)
        {
            var alreadyExists = await _badgeCategoryKudosTypesDbSet
                .AnyAsync(x => x.BadgeCategoryId == badgeCategoryId && x.KudosTypeId == kudosTypeId);

            if (alreadyExists)
            {
                throw new ValidationException(444, 
                    $"Badge category (ID {badgeCategoryId}) and kudos type (ID {kudosTypeId}) relationship already exists");
            }

            var entity = new BadgeCategoryKudosType
            {
                BadgeCategoryId = badgeCategoryId,
                KudosTypeId = kudosTypeId
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
                throw new ValidationException(444,
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

        public async Task<object> GetAllBadgeCategoriesAsync()
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
                                ?? throw new ValidationException(444, $"Badge category with ID {badgeCategoryId} not found");
            _badgeCategoriesDbSet.Remove(badgeCategory);
            await SaveChangesAsync();
        }

        public async Task DeleteBadgeCategoryFromKudosTypeAsync(int badgeCategoryId, int kudosTypeId)
        {
            var relationship = await _badgeCategoryKudosTypesDbSet
                                   .FirstOrDefaultAsync(x => x.BadgeCategoryId == badgeCategoryId && x.KudosTypeId == kudosTypeId)
                               ?? throw new ValidationException(444,
                                   $"Badge category (ID {badgeCategoryId}) and kudos type (ID {kudosTypeId}) relationship does not exist");

            _badgeCategoryKudosTypesDbSet.Remove(relationship);
            await SaveChangesAsync();
        }
        #endregion

        #region Private methods
        private BadgeType GetBadgeType(int badgeTypeId) 
            => _badgeTypesDbSet.Find(badgeTypeId)
                    ?? throw new ValidationException(444, $"Badge type with ID {badgeTypeId} not found");
        private async Task SaveChangesAsync()
        {
            var response = await _uow.SaveChangesAsync();
            if (response == 0)
            {
                throw new Exception("Changes could not be saved in BadgesService"); // TODO use appropriate exception
            }
        }
        #endregion
    }
}
