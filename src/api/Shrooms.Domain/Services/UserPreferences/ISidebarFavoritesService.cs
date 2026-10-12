using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Domain.Services.UserPreferences
{
    public interface ISidebarFavoritesService
    {
        Task<string> GetAsync(UserAndOrganizationDto userOrg);

        Task SaveAsync(string favorites, UserAndOrganizationDto userOrg);
    }
}
