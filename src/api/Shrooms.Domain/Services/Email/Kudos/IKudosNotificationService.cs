using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Email.Kudos
{
    public interface IKudosNotificationService
    {
        Task NotifyRejectedKudosLogSenderAsync(KudosLog kudosLog);
        Task NotifyAboutKudosSentAsync(AddKudosDto kudosDto);
        Task NotifyApprovedKudosRecipientAsync(KudosLog kudosLog);
        Task NotifyApprovedKudosDecreaseRecipientAsync(KudosLog kudosLog);
    }
}
