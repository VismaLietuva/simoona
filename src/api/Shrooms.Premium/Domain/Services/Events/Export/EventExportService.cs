using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private const string LabelSeparator = " + ";
        private const string PersonSeparator = ", ";

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
            var combinations = BuildCombinations(participants);

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(EventsConstants.EventParticipantsExcelTableName)
                .AddHeader(
                    Resources.Models.ApplicationUser.ApplicationUser.FirstName,
                    Resources.Models.ApplicationUser.ApplicationUser.LastName)
                .AddRows(participants.AsQueryable(), MapEventParticipantToExcelRow())
                .AutoFitColumns();

            if (combinations.Any())
            {
                excelBuilder
                    .AddWorksheet(EventsConstants.EventOptionsExcelTableName)
                    .AddHeader(
                        Resources.Models.Events.Events.Combination,
                        Resources.Models.Events.Events.Count,
                        Resources.Models.Events.Events.People)
                    .AddRows(MapCombinationsToExcelRows(combinations))
                    .AutoFitColumns();
            }

            var fileName = FileExportName.Sanitize($"{eventName} - participants", "event - participants", ".xlsx");
            return new FileExportDto(excelBuilder.Build(), fileName);
        }

        /// <summary>
        /// One row per distinct set of picks, which is what an organizer places an order from:
        /// "Deep dish + Margerita + Cheese, 2". Counting each option on its own loses which picks
        /// belonged together, so it cannot say whether the deep dish wanted margerita or marinara.
        /// </summary>
        private static List<EventCombination> BuildCombinations(IEnumerable<EventParticipantDto> participants)
        {
            var combinations = new Dictionary<string, EventCombination>();

            foreach (var participant in participants)
            {
                var choices = Ordered(participant.Choices);

                if (choices.Count == 0)
                {
                    continue;
                }

                var key = string.Join(">", choices.Select(choice => choice.OptionId));

                if (!combinations.TryGetValue(key, out var combination))
                {
                    combination = new EventCombination
                    {
                        Labels = string.Join(LabelSeparator, choices.Select(choice => choice.Option)),
                        Sequence = choices.SelectMany(SortKey).ToList()
                    };

                    combinations.Add(key, combination);
                }

                combination.People.Add(FullName(participant));
            }

            return combinations.Values
                .OrderBy(combination => combination, Comparer<EventCombination>.Create(
                    (left, right) => CompareSequences(left.Sequence, right.Sequence)))
                .ToList();
        }

        private static List<EventParticipantChoiceDto> Ordered(IEnumerable<EventParticipantChoiceDto> choices)
        {
            if (choices == null)
            {
                return new List<EventParticipantChoiceDto>();
            }

            return choices
                .OrderBy(choice => choice.QuestionId == null ? 0 : 1)
                .ThenBy(choice => choice.QuestionOrder ?? 0)
                .ThenBy(choice => choice.Order)
                .ThenBy(choice => choice.OptionId)
                .ToList();
        }

        private static IEnumerable<int> SortKey(EventParticipantChoiceDto choice)
        {
            yield return choice.QuestionId == null ? 0 : 1;
            yield return choice.QuestionOrder ?? 0;
            yield return choice.Order;
            yield return choice.OptionId;
        }

        // Orders the sheet the way the answers themselves are ordered, so a shorter order that
        // starts the same way sits directly above the longer one that extends it.
        private static int CompareSequences(IReadOnlyList<int> left, IReadOnlyList<int> right)
        {
            for (var i = 0; i < Math.Min(left.Count, right.Count); i++)
            {
                if (left[i] != right[i])
                {
                    return left[i].CompareTo(right[i]);
                }
            }

            return left.Count.CompareTo(right.Count);
        }

        private static string FullName(EventParticipantDto participant)
        {
            return $"{participant.FirstName} {participant.LastName}".Trim();
        }

        private static IExcelRowCollection MapCombinationsToExcelRows(IEnumerable<EventCombination> combinations)
        {
            var rows = new ExcelRowCollection();

            foreach (var combination in combinations)
            {
                rows.Add(new ExcelRow
                {
                    new ExcelColumn { Value = combination.Labels },
                    new ExcelColumn { Value = combination.People.Count.ToString() },
                    new ExcelColumn
                    {
                        Value = string.Join(
                            PersonSeparator,
                            combination.People.OrderBy(name => name, StringComparer.CurrentCulture))
                    }
                });
            }

            return rows;
        }

        private static Expression<Func<EventParticipantDto, IExcelRow>> MapEventParticipantToExcelRow()
        {
            return participant => new ExcelRow
            {
                new ExcelColumn
                {
                    Value = participant.FirstName
                },

                new ExcelColumn
                {
                    Value = participant.LastName
                }
            };
        }

        private sealed class EventCombination
        {
            public string Labels { get; set; }

            public List<int> Sequence { get; set; }

            public List<string> People { get; } = new List<string>();
        }
    }
}
