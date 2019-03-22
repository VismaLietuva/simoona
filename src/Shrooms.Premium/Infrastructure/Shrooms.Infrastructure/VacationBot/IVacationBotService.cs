using System.Threading.Tasks;

namespace Shrooms.Infrastructure.VacationBot
{
    public interface IVacationBotService
    {
        Task<VacationInfo[]> GetVacationHistory(string email);
    }
}