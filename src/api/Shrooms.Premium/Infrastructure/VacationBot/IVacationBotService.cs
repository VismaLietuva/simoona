using System.Threading.Tasks;

namespace Shrooms.Premium.Infrastructure.VacationBot
{
    public interface IVacationBotService
    {
        Task<VacationInfo[]> GetVacationHistory(string email);
    }
}