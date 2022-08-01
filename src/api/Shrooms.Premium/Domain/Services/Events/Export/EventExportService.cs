using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
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

        public async Task<ByteArrayContent> ExportOptionsAndParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var participants = await _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg);
            var options = await _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg);

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(EventsConstants.EventParticipantsExcelTableName)
                .AddHeader(
                    Resources.Models.ApplicationUser.ApplicationUser.FirstName,
                    Resources.Models.ApplicationUser.ApplicationUser.LastName)
                .AddRows(participants.AsQueryable(), MapEventParticipantToExcelRow())
                .AutoFitColumns();

            if (!options.Any())
            {
                return new ByteArrayContent(excelBuilder.Build());
            }

            excelBuilder
                .AddWorksheet(EventsConstants.EventOptionsExcelTableName)
                .AddHeader(
                    Resources.Models.Events.Events.Option,
                    Resources.Models.Events.Events.Count)
                .AddRows(options.AsQueryable(), MapEventOptionToExcelRow())
                .AutoFitColumns();

            return new ByteArrayContent(excelBuilder.Build());
        }

        private static Expression<Func<EventOptionCountDto, IExcelRow>> MapEventOptionToExcelRow()
        {
            return option => new ExcelRow
            {
                Columns = new List<IExcelColumn>
                {
                    new ExcelColumn
                    {
                        Value = option.Option
                    },

                    new ExcelColumn
                    {
                        Value = option.Count,
                        Format = ExcelWorksheetBuilderConstants.NumberFormat
                    }
                }
            };
        }

        private static Expression<Func<EventParticipantDto, IExcelRow>> MapEventParticipantToExcelRow()
        {
            return participant => new ExcelRow
            {
                Columns = new List<IExcelColumn>
                {
                    new ExcelColumn
                    {
                        Value = participant.FirstName
                    },

                    new ExcelColumn
                    {
                        Value = participant.LastName
                    }
                }
            };
        }
    }
}