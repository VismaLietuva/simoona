using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization
{
    public interface IUsersAnonymizationWebHookService
    {
        Task AnonymizeUsersAsync(int organizationId);
    }
}