using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.Services.Events.Export;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventExportServiceTests
    {
        private IEventUtilitiesService _eventUtilitiesService;
        private IEventParticipationService _eventParticipationService;
        private IEventExportService _eventExportService;
        private IExcelBuilderFactory _excelBuilder;

        [SetUp]
        public void TestInitializer()
        {
            _eventParticipationService = Substitute.For<IEventParticipationService>();
            _eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            _excelBuilder = new ExcelBuilderFactory();

            _eventExportService = new EventExportService(_eventParticipationService, _eventUtilitiesService, _excelBuilder);
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Participant_Names_Only()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = MockParticipantsWithQuestions(userAndOrg);

            var tables = await ExportTablesAsync(eventId, userAndOrg);
            var participants = tables[0];

            ClassicAssert.AreEqual(2, participants.Columns.Count);
            ClassicAssert.AreEqual("First name", participants.Columns[0].ColumnName);
            ClassicAssert.AreEqual("Last name", participants.Columns[1].ColumnName);

            ClassicAssert.AreEqual("Ada", participants.Rows[0].ItemArray[0]);
            ClassicAssert.AreEqual("Lovelace", participants.Rows[0].ItemArray[1]);
        }

        [Test]
        public async Task Should_Write_One_Row_Per_Distinct_Combination()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = MockParticipantsWithQuestions(userAndOrg);

            var tables = await ExportTablesAsync(eventId, userAndOrg);
            var options = tables[1];

            ClassicAssert.AreEqual(2, tables.Count);
            ClassicAssert.AreEqual("Combination", options.Columns[0].ColumnName);
            ClassicAssert.AreEqual("Count", options.Columns[1].ColumnName);
            ClassicAssert.AreEqual("People", options.Columns[2].ColumnName);

            ClassicAssert.AreEqual(2, options.Rows.Count);

            ClassicAssert.AreEqual("Pizza + Vegan + M", options.Rows[0].ItemArray[0]);
            ClassicAssert.AreEqual("2", options.Rows[0].ItemArray[1]);
            ClassicAssert.AreEqual("Ada Lovelace, Grace Hopper", options.Rows[0].ItemArray[2]);

            ClassicAssert.AreEqual("Pizza + M + L", options.Rows[1].ItemArray[0]);
            ClassicAssert.AreEqual("1", options.Rows[1].ItemArray[1]);
            ClassicAssert.AreEqual("Alan Turing", options.Rows[1].ItemArray[2]);
        }

        [Test]
        public async Task Should_Not_Name_The_Questions_A_Combination_Came_From()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = MockParticipantsWithQuestions(userAndOrg);

            var tables = await ExportTablesAsync(eventId, userAndOrg);

            var everyCell = tables[1].Rows.Cast<DataRow>()
                .SelectMany(row => row.ItemArray)
                .Select(value => value?.ToString() ?? string.Empty);

            CollectionAssert.DoesNotContain(everyCell, "Dietary needs");
            CollectionAssert.DoesNotContain(everyCell, "T-shirt size");
        }

        [Test]
        public async Task Should_Treat_A_Flat_Only_Event_As_Single_Answer_Combinations()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = MockFlatOnlyParticipants(userAndOrg);

            var tables = await ExportTablesAsync(eventId, userAndOrg);
            var options = tables[1];

            ClassicAssert.AreEqual(2, options.Rows.Count);

            ClassicAssert.AreEqual("Pizza", options.Rows[0].ItemArray[0]);
            ClassicAssert.AreEqual("2", options.Rows[0].ItemArray[1]);
            ClassicAssert.AreEqual("Ada Lovelace, Grace Hopper", options.Rows[0].ItemArray[2]);

            ClassicAssert.AreEqual("Salad", options.Rows[1].ItemArray[0]);
            ClassicAssert.AreEqual("1", options.Rows[1].ItemArray[1]);
            ClassicAssert.AreEqual("Alan Turing", options.Rows[1].ItemArray[2]);
        }

        [Test]
        public async Task Should_List_Participants_In_A_Stable_Order()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = MockParticipantsWithQuestions(userAndOrg);

            var tables = await ExportTablesAsync(eventId, userAndOrg);

            var names = tables[0].Rows.Cast<DataRow>()
                .Select(row => row.ItemArray[0]?.ToString())
                .ToArray();

            // Seeded Ada, Grace, Alan — the sheet cannot inherit the query's arbitrary order.
            ClassicAssert.AreEqual(new[] { "Ada", "Alan", "Grace" }, names);
        }

        [Test]
        public async Task Should_Trim_A_People_Cell_That_Would_Break_The_Workbook()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = Guid.NewGuid();

            // Enough identical picks to push the joined names past Excel's per-cell ceiling.
            var crowd = Enumerable.Range(1000, 2600).Select(i => new EventParticipantDto
            {
                FirstName = $"Person{i}",
                LastName = "Attendee",
                Choices = new List<EventParticipantChoiceDto> { Choice(10, "Pizza") }
            }).ToList();

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(crowd);

            var tables = await ExportTablesAsync(eventId, userAndOrg);
            var cell = tables[1].Rows[0].ItemArray[2]?.ToString();

            ClassicAssert.LessOrEqual(cell.Length, 32767);
            StringAssert.Contains("more)", cell);
            ClassicAssert.AreEqual("2600", tables[1].Rows[0].ItemArray[1]);
            StringAssert.DoesNotContain("Person1000,", cell.Substring(cell.Length - 20));
        }

        [Test]
        public async Task Should_Omit_The_Options_Sheet_When_Nobody_Picked_Anything()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = Guid.NewGuid();

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(new List<EventParticipantDto>
            {
                new EventParticipantDto { FirstName = "Ada", LastName = "Lovelace" }
            });

            var tables = await ExportTablesAsync(eventId, userAndOrg);

            ClassicAssert.AreEqual(1, tables.Count);
        }

        [Test]
        public async Task Should_Leave_Out_Participants_Who_Picked_Nothing()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };
            var eventId = Guid.NewGuid();

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        Choice(1, "Pizza")
                    }
                },
                new EventParticipantDto { FirstName = "Silent", LastName = "Bob" }
            });

            var tables = await ExportTablesAsync(eventId, userAndOrg);

            ClassicAssert.AreEqual(2, tables[0].Rows.Count);
            ClassicAssert.AreEqual(1, tables[1].Rows.Count);
            ClassicAssert.AreEqual("Ada Lovelace", tables[1].Rows[0].ItemArray[2]);
        }

        private async Task<DataTableCollection> ExportTablesAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(eventId, userAndOrg);

            using var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content));

            return excelReader
                .AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                })
                .Tables;
        }

        private static EventParticipantChoiceDto Choice(int optionId, string option, int? questionOrder = null, int order = 0)
        {
            return new EventParticipantChoiceDto
            {
                OptionId = optionId,
                Option = option,
                QuestionOrder = questionOrder,
                Order = order
            };
        }

        private Guid MockParticipantsWithQuestions(UserAndOrganizationDto userAndOrg)
        {
            var eventId = Guid.NewGuid();


            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    // Stored out of order on purpose: the sheet has to sort them.
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        Choice(30, "M", questionOrder: 1),
                        Choice(10, "Pizza"),
                        Choice(20, "Vegan", questionOrder: 0)
                    }
                },
                new EventParticipantDto
                {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        Choice(10, "Pizza"),
                        Choice(20, "Vegan", questionOrder: 0),
                        Choice(30, "M", questionOrder: 1)
                    }
                },
                new EventParticipantDto
                {
                    FirstName = "Alan",
                    LastName = "Turing",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        Choice(10, "Pizza"),
                        Choice(30, "M", questionOrder: 1),
                        Choice(31, "L", questionOrder: 1, order: 1)
                    }
                }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            return eventId;
        }

        private Guid MockFlatOnlyParticipants(UserAndOrganizationDto userAndOrg)
        {
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Choices = new List<EventParticipantChoiceDto> { Choice(10, "Pizza") }
                },
                new EventParticipantDto
                {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Choices = new List<EventParticipantChoiceDto> { Choice(10, "Pizza") }
                },
                new EventParticipantDto
                {
                    FirstName = "Alan",
                    LastName = "Turing",
                    Choices = new List<EventParticipantChoiceDto> { Choice(11, "Salad", order: 1) }
                }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            return eventId;
        }
    }
}
