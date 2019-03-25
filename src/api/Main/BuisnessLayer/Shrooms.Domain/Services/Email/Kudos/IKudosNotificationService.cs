using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.EntityModels.Models.Kudos;

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
