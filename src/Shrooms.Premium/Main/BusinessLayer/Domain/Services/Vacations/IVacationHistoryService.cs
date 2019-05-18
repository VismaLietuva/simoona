using System.Threading.Tasks;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Vacations
{
    public interface IVacationHistoryService
    {
        Task<VacationDTO[]> GetVacationHistory(string userId);
    }
}