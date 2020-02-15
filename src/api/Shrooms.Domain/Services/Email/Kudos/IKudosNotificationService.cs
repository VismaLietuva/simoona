using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Email.Kudos
{
    public interface IKudosNotificationService
    {
        void NotifyRejectedKudosLogSender(KudosLog kudosLog);
        void NotifyAboutKudosSent(AddKudosDTO kudosDto);
        void NotifyApprovedKudosRecipient(KudosLog kudosLog);
        void NotifyApprovedKudosDecreaseRecipient(KudosLog kudosLog);
    }
}
