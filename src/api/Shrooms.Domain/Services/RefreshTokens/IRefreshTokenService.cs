using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.RefreshTokens;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.RefreshTokens
{
    public interface IRefreshTokenService
    {
        Task RemoveTokenByIdAsync(string id);
        Task<RefreshToken> GetTokenTicketByIdAsync(string id);
        Task AddNewTokenAsync(RefreshTokenDto tokenDto);
        Task RemoveTokenBySubjectAsync(UserAndOrganizationDto userOrg);
    }
}
