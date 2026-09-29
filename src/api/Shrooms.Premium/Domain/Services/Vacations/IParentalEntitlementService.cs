using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IParentalEntitlementService
    {
        Task<ParentalBalanceDto> GetBalanceAsync(UserAndOrganizationDto userOrg);

        Task<IList<ParentalEntitlementDto>> GetAllAsync(ParentalEntitlementListArgsDto args);

        Task<ParentalEntitlementDto> SetAsync(string employeeId, string type, UserAndOrganizationDto userOrg);
    }
}
