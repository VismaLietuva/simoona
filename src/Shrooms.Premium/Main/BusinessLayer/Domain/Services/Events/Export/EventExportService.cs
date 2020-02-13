using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Participation;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Utilities;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Export
{
    public class EventExportService : IEventExportService
    {
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IExcelBuilder _excelBuilder;

        public EventExportService(
            IEventParticipationService eventParticipationService,
            IEventUtilitiesService eventUtilitiesService,
            IExcelBuilder excelBuilder)
        {
            _eventParticipationService = eventParticipationService;
            _eventUtilitiesService = eventUtilitiesService;
            _excelBuilder = excelBuilder;
        }

        public byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg)
        {
            var participants = _eventParticipationService
                .GetEventParticipants(eventId, userAndOrg)
                .Select(x => new List<string> { x.FirstName, x.LastName });

            var options = _eventUtilitiesService.GetEventChosenOptions(eventId, userAndOrg)
                .Select(x => new List<string> { x.Option, x.Count.ToString() });

            AddParticipants(participants);

            if (options.Any())
            {
                AddOptions(options);
            }

            return _excelBuilder.GenerateByteArray();
        }

        private void AddOptions(IEnumerable<List<string>> options)
        {
            var header = new List<string>
            {
                Resources.Models.Events.Events.Option,
                Resources.Models.Events.Events.Count
            };

            _excelBuilder.AddNewWorksheet(EventsConstants.EventOptionsExcelTableName, header, options);
        }

        private void AddParticipants(IEnumerable<List<string>> participants)
        {
            var header = new List<string>
            {
                Resources.Models.ApplicationUser.ApplicationUser.FirstName,
                Resources.Models.ApplicationUser.ApplicationUser.LastName
            };

            _excelBuilder.AddNewWorksheet(EventsConstants.EventParticipantsExcelTableName, header, participants);
        }
    }
}