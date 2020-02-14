using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Badges;

namespace Shrooms.Premium.Domain.Services.Badges
{
    public interface IBadgesService
    {
        Task AddBadgeTypeAsync(string title,
                               string description,
                               int value,
                               string imageUrl,
                               string imageSmallUrl,
                               int badgeCategoryId);

        Task AddBadgeCategoryAsync(string title, string description);

        Task AddBadgeCategoryToKudosTypeAsync(int badgeCategoryId, int kudosTypeId, BadgeCalculationPolicyType calculationPolicy);
        Task AddBadgeToUserAsync(int badgeTypeId, string employeeId, int organizationId);
        Task<IList<BadgeCategory>> GetAllBadgeCategoriesAsync();
        Task ActivateBadgeType(int badgeTypeId);
        Task DeactivateBadgeType(int badgeTypeId);
        Task DeleteBadgeTypeAsync(int badgeTypeId);
        Task DeleteBadgeCategoryAsync(int badgeCategoryId);
        Task DeleteBadgeCategoryFromKudosTypeAsync(int badgeCategoryId, int kudosTypeId);
        Task AssignBadgesAsync();
    }
}