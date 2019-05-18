using System.Threading.Tasks;

namespace Shrooms.Premium.Infrastructure.Shrooms.Infrastructure.VacationBot
{
    public interface IVacationBotService
    {
        Task<VacationInfo[]> GetVacationHistory(string email);
    }
}