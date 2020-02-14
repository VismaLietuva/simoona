using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.RefreshTokens;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.RefreshTokens
{
    public interface IRefreshTokenService
    {
        void RemoveTokenById(string id);
        RefreshToken GetTokenTicketById(string id);
        void AddNewToken(RefreshTokenDTO tokenDto);
        void RemoveTokenBySubject(UserAndOrganizationDTO userOrg);
    }
}
