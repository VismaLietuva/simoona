using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Groups;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Groups
{
    public interface IGroupKudosWebHookService
    {
        Task<IList<GroupMonthlyKudosResultDto>> AwardMonthlyGroupKudosAsync(string organizationName, int? year = null, int? month = null);
    }
}
