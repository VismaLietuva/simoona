using System.Threading.Tasks;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Vacations
{
    public interface IVacationHistoryService
    {
        Task<VacationDTO[]> GetVacationHistory(string userId);
    }
}