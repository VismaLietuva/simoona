using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Events.Participation;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Domain.Services.Events.Export
{
    public class EventExportService : IEventExportService
    {
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventUtilitiesService _eventUtlitiesService;
        private readonly IExcelBuilder _excelBuilder;

        public EventExportService(
            IEventParticipationService eventParticipationService,
            IEventUtilitiesService eventUtilitiesService,
            IExcelBuilder excelBuilder)
        {
            _eventParticipationService = eventParticipationService;
            _eventUtlitiesService = eventUtilitiesService;
            _excelBuilder = excelBuilder;
        }
        public byte[] ExportOptionsAndParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg)
        {
            var participants = _eventParticipationService
                .GetEventParticipants(eventId, userAndOrg)
                .Select(x => new List<string> { x.FirstName, x.LastName });

            var options = _eventUtlitiesService.GetEventChosenOptions(eventId, userAndOrg)
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

            _excelBuilder.AddNewWorksheet(
                ConstBusinessLayer.EventOptionsExcelTableName,
                header,
                options);
        }

        private void AddParticipants(IEnumerable<List<string>> participants)
        {
            var header = new List<string>
            {
                Resources.Models.ApplicationUser.ApplicationUser.FirstName,
                Resources.Models.ApplicationUser.ApplicationUser.LastName
            };

            _excelBuilder.AddNewWorksheet(
                ConstBusinessLayer.EventParticipantsExcelTableName,
                header,
                participants);
        }
    }
}
