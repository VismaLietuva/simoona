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

        private static readonly IComparer<IReadOnlyList<int>> BySequence =
            Comparer<IReadOnlyList<int>>.Create(CompareSequences);

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
                var choices = (participant.Choices ?? Enumerable.Empty<EventParticipantChoiceDto>())
                    .OrderBy(Rank, BySequence)
                    .ToList();

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
                        Sequence = choices.SelectMany(Rank).ToList()
                    };

                    combinations.Add(key, combination);
                }

                combination.People.Add(FullName(participant));
            }

            return combinations.Values
                .OrderBy(combination => combination.Sequence, BySequence)
                .ToList();
        }

        /// <summary>
        /// Where one pick sorts, both inside a row and between rows: flat options first, then by
        /// question, then by option. Id settles the rest, because every legacy flat option is
        /// written with Order 0 and would otherwise tie.
        /// </summary>
        private static IReadOnlyList<int> Rank(EventParticipantChoiceDto choice)
        {
            return new[]
            {
                choice.QuestionOrder == null ? 0 : 1,
                choice.QuestionOrder ?? 0,
                choice.Order,
                choice.OptionId
            };
        }

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
