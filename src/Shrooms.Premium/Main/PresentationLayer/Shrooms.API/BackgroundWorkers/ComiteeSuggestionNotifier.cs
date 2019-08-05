using Shrooms.DataTransferObjects.Models.Committees;
using Shrooms.Domain.Services.Email.Committee;
using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Infrastructure.Logger;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
{
    public class ComiteeSuggestionNotifier: IBackgroundWorker
    {
        private readonly ILogger _logger;
        private readonly ICommitteeNotificationService _committeeNotificationService;

        public ComiteeSuggestionNotifier()
        {

        }

        public void Notify(ComiteeSuggestionCreatedDto createdDto)
        {
            try
            {
                _committeeNotificationService.NotifyCommitteeMembersAboutNewSuggestion(createdDto);
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
