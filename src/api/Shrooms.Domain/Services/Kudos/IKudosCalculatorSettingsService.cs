using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosCalculatorSettingsService
    {
        Task<KudosCalculatorSettingsDto> GetAsync(UserAndOrganizationDto userOrg);

        Task UpdateAsync(KudosCalculatorSettingsDto input, UserAndOrganizationDto userOrg);
    }
}
