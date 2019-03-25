using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.RefreshTokens;
using Shrooms.EntityModels.Models;

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
