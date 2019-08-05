using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Logger;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers
{
    public class RemovedParticipantsNotifier : IBackgroundWorker
    {
        private readonly ILogger _logger;
        private readonly IEventCalendarService _calendarService;
        private readonly IEventNotificationService _eventNotificationService;
        public RemovedParticipantsNotifier(ILogger logger, IEventCalendarService calendarService, IEventNotificationService eventNotificationService)
        {
            _logger = logger;
            _calendarService = calendarService;
            _eventNotificationService = eventNotificationService;
        }
        public void NotifyOnEventReset(EventParticipantsChangeDto participantsChange, UserAndOrganizationDTO userOrg)
        {
            try
            {
                _eventNotificationService.NotifyRemovedEventParticipants(participantsChange.EventName, participantsChange.EventId, userOrg.OrganizationId, participantsChange.RemovedUsers);
                _calendarService.ResetParticipants(participantsChange.EventId, userOrg.OrganizationId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void NotifyOnUserRemoval(EventParticipantsChangeDto participantsChange, UserAndOrganizationDTO userOrg)
        {
            try
            {
                _calendarService.RemoveParticipants(participantsChange.EventId, userOrg.OrganizationId, participantsChange.RemovedUsers);
                _eventNotificationService.NotifyRemovedEventParticipants(
                    participantsChange.EventName,
                    participantsChange.EventId,
                    userOrg.OrganizationId,
                    participantsChange.RemovedUsers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
