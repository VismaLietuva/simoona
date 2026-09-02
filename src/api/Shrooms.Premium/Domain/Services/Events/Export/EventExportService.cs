using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private const string AnswerSeparator = ", ";

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

        public async Task<FileExportDto> ExportOptionsAndParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var eventName = await _eventUtilitiesService.GetEventNameAsync(eventId);
            var participants = (await _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg)).ToList();
            var options = (await _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg)).ToList();

            // Both sheets take their column order from this one list, which the query already sorts
            // flat options first, then by question order.
            var choiceColumns = GetChoiceColumns(options);

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(EventsConstants.EventParticipantsExcelTableName)
                .AddHeader(GetParticipantsHeader(choiceColumns))
                .AddRows(MapParticipantsToExcelRows(participants, choiceColumns))
                .AutoFitColumns();

            if (options.Any())
            {
                excelBuilder
                    .AddWorksheet(EventsConstants.EventOptionsExcelTableName)
                    .AddHeader(
                        Resources.Models.Events.Events.Question,
                        Resources.Models.Events.Events.Option,
                        Resources.Models.Events.Events.Count)
                    .AddRows(MapOptionsToExcelRows(options))
                    .AutoFitColumns();
            }

            var fileName = FileExportName.Sanitize($"{eventName} - participants", "event - participants", ".xlsx");
            return new FileExportDto(excelBuilder.Build(), fileName);
        }

        private static List<EventOptionCountDto> GetChoiceColumns(IEnumerable<EventOptionCountDto> options)
        {
            return options
                .GroupBy(option => option.QuestionId)
                .Select(group => group.First())
                .ToList();
        }

        private static IEnumerable<string> GetParticipantsHeader(IEnumerable<EventOptionCountDto> choiceColumns)
        {
            var header = new List<string>
            {
                Resources.Models.ApplicationUser.ApplicationUser.FirstName,
                Resources.Models.ApplicationUser.ApplicationUser.LastName
            };

            header.AddRange(choiceColumns.Select(column => column.Question ?? Resources.Models.Events.Events.Option));

            return header;
        }

        private static IExcelRowCollection MapParticipantsToExcelRows(
            IEnumerable<EventParticipantDto> participants,
            IReadOnlyCollection<EventOptionCountDto> choiceColumns)
        {
            var rows = new ExcelRowCollection();

            foreach (var participant in participants)
            {
                var row = new ExcelRow
                {
                    new ExcelColumn { Value = participant.FirstName },
                    new ExcelColumn { Value = participant.LastName }
                };

                foreach (var column in choiceColumns)
                {
                    row.Add(new ExcelColumn { Value = GetAnswer(participant, column.QuestionId) });
                }

                rows.Add(row);
            }

            return rows;
        }

        private static string GetAnswer(EventParticipantDto participant, int? questionId)
        {
            if (participant.Choices == null)
            {
                return string.Empty;
            }

            // Option breaks the Order tie that every legacy flat choice shares, so the same pair of
            // picks cannot render in one order on one row and the reverse on the next.
            var answers = participant.Choices
                .Where(choice => choice.QuestionId == questionId)
                .OrderBy(choice => choice.Order)
                .ThenBy(choice => choice.Option)
                .Select(choice => choice.Option);

            return string.Join(AnswerSeparator, answers);
        }

        private static IExcelRowCollection MapOptionsToExcelRows(IEnumerable<EventOptionCountDto> options)
        {
            var rows = new ExcelRowCollection();

            foreach (var option in options)
            {
                rows.Add(new ExcelRow
                {
                    new ExcelColumn { Value = option.Question ?? string.Empty },
                    new ExcelColumn { Value = option.Option },
                    new ExcelColumn { Value = option.Count.ToString() }
                });
            }

            return rows;
        }
    }
}
