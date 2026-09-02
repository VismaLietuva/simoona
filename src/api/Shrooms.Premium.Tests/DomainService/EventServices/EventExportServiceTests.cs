using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task Should_Return_Excel_File_With_Participants()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;

                ClassicAssert.AreEqual("Name", excelRows[0].ItemArray[0]);
                ClassicAssert.AreEqual("Surname", excelRows[0].ItemArray[1]);
            }
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Participants_And_Without_Options()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithoutOptionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                ClassicAssert.AreEqual(1, excelData.Tables.Count);
            }
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Options()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var optionsTable = excelData.Tables[1];
                var excelRows = optionsTable.Rows;

                ClassicAssert.AreEqual(2, excelData.Tables.Count);
                ClassicAssert.AreEqual("Question", optionsTable.Columns[0].ColumnName);
                ClassicAssert.AreEqual("Option", optionsTable.Columns[1].ColumnName);
                ClassicAssert.AreEqual("Count", optionsTable.Columns[2].ColumnName);
                ClassicAssert.AreEqual("Option1", excelRows[0].ItemArray[1]);
                ClassicAssert.AreEqual("2", excelRows[0].ItemArray[2]);
                ClassicAssert.AreEqual("Option2", excelRows[1].ItemArray[1]);
                ClassicAssert.AreEqual("1", excelRows[1].ItemArray[2]);
                ClassicAssert.AreEqual(2, excelRows.Count);
            }
        }

        [Test]
        public async Task Should_Name_The_Question_Each_Option_Belongs_To()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithQuestionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[1].Rows;

                ClassicAssert.AreEqual(4, excelRows.Count);

                ClassicAssert.AreEqual(string.Empty, CellText(excelRows[0].ItemArray[0]));
                ClassicAssert.AreEqual("Pizza", excelRows[0].ItemArray[1]);
                ClassicAssert.AreEqual("2", excelRows[0].ItemArray[2]);

                ClassicAssert.AreEqual("Dietary needs", excelRows[1].ItemArray[0]);
                ClassicAssert.AreEqual("Vegan", excelRows[1].ItemArray[1]);
                ClassicAssert.AreEqual("1", excelRows[1].ItemArray[2]);

                ClassicAssert.AreEqual("T-shirt size", excelRows[2].ItemArray[0]);
                ClassicAssert.AreEqual("M", excelRows[2].ItemArray[1]);
                ClassicAssert.AreEqual("1", excelRows[2].ItemArray[2]);

                ClassicAssert.AreEqual("T-shirt size", excelRows[3].ItemArray[0]);
                ClassicAssert.AreEqual("L", excelRows[3].ItemArray[1]);
                ClassicAssert.AreEqual("1", excelRows[3].ItemArray[2]);
            }
        }

        [Test]
        public async Task Should_Return_Excel_File_With_A_Column_Per_Question_Holding_Each_Participants_Answer()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithQuestionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var participantsTable = excelData.Tables[0];
                var excelRows = participantsTable.Rows;

                ClassicAssert.AreEqual(5, participantsTable.Columns.Count);
                ClassicAssert.AreEqual("Option", participantsTable.Columns[2].ColumnName);
                ClassicAssert.AreEqual("Dietary needs", participantsTable.Columns[3].ColumnName);
                ClassicAssert.AreEqual("T-shirt size", participantsTable.Columns[4].ColumnName);

                ClassicAssert.AreEqual("Ada", excelRows[0].ItemArray[0]);
                ClassicAssert.AreEqual("Lovelace", excelRows[0].ItemArray[1]);
                ClassicAssert.AreEqual("Pizza", excelRows[0].ItemArray[2]);
                ClassicAssert.AreEqual("Vegan", excelRows[0].ItemArray[3]);
                ClassicAssert.AreEqual("M", excelRows[0].ItemArray[4]);

                ClassicAssert.AreEqual("Grace", excelRows[1].ItemArray[0]);
                ClassicAssert.AreEqual("Hopper", excelRows[1].ItemArray[1]);
                ClassicAssert.AreEqual("Pizza", excelRows[1].ItemArray[2]);
                ClassicAssert.AreEqual(string.Empty, CellText(excelRows[1].ItemArray[3]));
                ClassicAssert.AreEqual("M, L", excelRows[1].ItemArray[4]);
            }
        }

        [Test]
        public async Task Should_Join_Answers_Alphabetically_When_They_Share_An_Order()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        new EventParticipantChoiceDto { Option = "Salad", Order = 0 },
                        new EventParticipantChoiceDto { Option = "Pizza", Order = 0 }
                    }
                },
                new EventParticipantDto
                {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        new EventParticipantChoiceDto { Option = "Pizza", Order = 0 },
                        new EventParticipantChoiceDto { Option = "Salad", Order = 0 }
                    }
                }
            };

            var options = new List<EventOptionCountDto>
            {
                new EventOptionCountDto { Option = "Pizza", Count = 2 },
                new EventOptionCountDto { Option = "Salad", Count = 2 }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(eventId, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;

                ClassicAssert.AreEqual("Pizza, Salad", excelRows[0].ItemArray[2]);
                ClassicAssert.AreEqual("Pizza, Salad", excelRows[1].ItemArray[2]);
            }
        }

        [Test]
        public async Task Should_Add_Only_The_Flat_Option_Column_When_The_Event_Has_No_Questions()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var export = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });

                ClassicAssert.AreEqual(3, excelData.Tables[0].Columns.Count);
            }
        }

        // A blank cell reads back as DBNull or "" depending on how EPPlus stored it.
        private static string CellText(object value)
        {
            return value == null || value is DBNull ? string.Empty : value.ToString();
        }

        private Guid MockParticipantsWithQuestionsForExport(UserAndOrganizationDto userAndOrg)
        {
            var eventId = Guid.NewGuid();

            const int dietaryNeedsId = 7;
            const int tShirtSizeId = 9;

            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        new EventParticipantChoiceDto { Option = "M", Order = 0, QuestionId = tShirtSizeId },
                        new EventParticipantChoiceDto { Option = "Vegan", Order = 0, QuestionId = dietaryNeedsId },
                        new EventParticipantChoiceDto { Option = "Pizza", Order = 0 }
                    }
                },
                new EventParticipantDto
                {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Choices = new List<EventParticipantChoiceDto>
                    {
                        new EventParticipantChoiceDto { Option = "L", Order = 1, QuestionId = tShirtSizeId },
                        new EventParticipantChoiceDto { Option = "M", Order = 0, QuestionId = tShirtSizeId },
                        new EventParticipantChoiceDto { Option = "Pizza", Order = 0 }
                    }
                }
            };

            var options = new List<EventOptionCountDto>
            {
                new EventOptionCountDto { Option = "Pizza", Count = 2 },
                new EventOptionCountDto { Option = "Vegan", Count = 1, QuestionId = dietaryNeedsId, Question = "Dietary needs" },
                new EventOptionCountDto { Option = "M", Count = 1, QuestionId = tShirtSizeId, Question = "T-shirt size" },
                new EventOptionCountDto { Option = "L", Count = 1, QuestionId = tShirtSizeId, Question = "T-shirt size" }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);
            return eventId;
        }

        private Guid MockParticipantsWithOptionsForExport(UserAndOrganizationDto userAndOrg)
        {
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Name",
                    LastName = "Surname"
                }
            };

            var options = new List<EventOptionCountDto>
            {
                new EventOptionCountDto
                {
                   Option = "Option1",
                   Count = 2
                },
                new EventOptionCountDto
                {
                   Option = "Option2",
                   Count = 1
                }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);
            return eventId;
        }

        private Guid MockParticipantsWithoutOptionsForExport(UserAndOrganizationDto userAndOrg)
        {
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDto>
            {
                new EventParticipantDto
                {
                    FirstName = "Name",
                    LastName = "Surname"
                }
            };

            // ReSharper disable once CollectionNeverUpdated.Local
            var options = new List<EventOptionCountDto>();

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);
            return eventId;
        }
    }
}
