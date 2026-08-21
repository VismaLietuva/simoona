using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationLogService
    {
        Task<IPagedList<VacationEventDto>> GetLogAsync(VacationLogArgsDto args);
    }
}
