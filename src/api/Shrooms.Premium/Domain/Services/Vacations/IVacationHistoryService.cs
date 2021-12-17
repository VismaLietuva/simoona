using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationHistoryService
    {
        Task<VacationDto[]> GetVacationHistoryAsync(string userId);
    }
}