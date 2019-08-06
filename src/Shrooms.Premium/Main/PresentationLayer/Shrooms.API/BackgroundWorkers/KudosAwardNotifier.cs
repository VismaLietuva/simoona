using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Infrastructure.Logger;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
{
    public class KudosAwardNotifier : IBackgroundWorker
    {
        private readonly ILogger _logger;
        private readonly IKudosPremiumNotificationService _notificationSrvc;
        public KudosAwardNotifier(ILogger logger)
        {
            _logger = logger;
        }

        public void Notify(List<AwardedKudosEmployeeDTO> awardedEmployees)
        {
            try
            {
                if(awardedEmployees.Count>0)
                    _notificationSrvc.SendLoyaltyBotNotification(awardedEmployees);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
