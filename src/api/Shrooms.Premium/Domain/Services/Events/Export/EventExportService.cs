using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;

namespace Shrooms.Premium.Domain.Services.Events.Export
{
    public class EventExportService : IEventExportService
    {
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IExcelBuilderFactory _excelBuilderFactory;

        public EventExportService(
            IEventParticipationService eventParticipationService,
            IEventUtilitiesService eventUtilitiesService,
            IExcelBuilderFactory excelBuilderFactory)
        {
            _eventParticipationService = eventParticipationService;
            _eventUtilitiesService = eventUtilitiesService;
            _excelBuilderFactory = excelBuilderFactory;
        }

        public async Task<byte[]> ExportOptionsAndParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var participants = (await _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg))
                .Select(x => new List<string> { x.FirstName, x.LastName });

            var options = (await _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg))
                .Select(x => new List<string> { x.Option, x.Count.ToString() })
                .ToList();

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(EventsConstants.EventParticipantsExcelTableName)
                .AddHeader(
                    Resources.Models.ApplicationUser.ApplicationUser.FirstName, 
                    Resources.Models.ApplicationUser.ApplicationUser.LastName)
                .AddRows(participants)
                .AutoFitColumns();

            if (!options.Any())
            {
                return excelBuilder.Build();
            }

            excelBuilder
                .AddWorksheet(EventsConstants.EventOptionsExcelTableName)
                .AddHeader(
                    Resources.Models.Events.Events.Option,
                    Resources.Models.Events.Events.Count)
                .AddRows(options)
                .AutoFitColumns();

            return excelBuilder.Build();
        }
    }
}