using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Domain.Services.Wall.Widgets
{
    public interface IWallWidgetPreferencesService
    {
        Task<string> GetAsync(UserAndOrganizationDto userOrg);

        Task SaveAsync(string preferences, UserAndOrganizationDto userOrg);
    }
}
