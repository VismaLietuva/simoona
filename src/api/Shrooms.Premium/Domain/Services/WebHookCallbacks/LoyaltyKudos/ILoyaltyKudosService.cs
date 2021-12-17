using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosService
    {
        Task AwardEmployeesWithKudosAsync(string organizationName);
    }
}