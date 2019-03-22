using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models.Vacations;

namespace Shrooms.Domain.Services.Vacations
{
    public interface IVacationHistoryService
    {
        Task<VacationDTO[]> GetVacationHistory(string userId);
    }
}