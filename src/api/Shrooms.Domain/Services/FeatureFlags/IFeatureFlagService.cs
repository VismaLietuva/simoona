using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags;

namespace Shrooms.Domain.Services.FeatureFlags
{
    public interface IFeatureFlagService
    {
        Task<IEnumerable<FeatureFlagDto>> GetAllAsync(UserAndOrganizationDto userOrg);

        Task<IEnumerable<string>> GetEnabledAsync(UserAndOrganizationDto userOrg);

        Task<int> CreateAsync(NewFeatureFlagDto input, UserAndOrganizationDto userOrg);

        Task UpdateAsync(int id, UpdateFeatureFlagDto input, UserAndOrganizationDto userOrg);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);
    }
}
