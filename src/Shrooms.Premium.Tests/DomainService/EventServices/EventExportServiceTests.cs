using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Excel;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
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
        private IExcelBuilder _excelBuilder;

        [SetUp]
        public void TestInitializer()
        {
            _eventParticipationService = Substitute.For<IEventParticipationService>();
            _eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            _excelBuilder = new ExcelBuilder();

            _eventExportService = new EventExportService(_eventParticipationService, _eventUtilitiesService, _excelBuilder);
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Participants()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var stream = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(stream)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[0].Rows;

                Assert.AreEqual("Name", excelRows[0].ItemArray[0]);
                Assert.AreEqual("Surname", excelRows[0].ItemArray[1]);

                excelReader.Close();
            }
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Participants_And_Without_Options()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithoutOptionsForExport(userAndOrg);

            var stream = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(stream)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                Assert.AreEqual(1, excelData.Tables.Count);

                excelReader.Close();
            }
        }

        [Test]
        public async Task Should_Return_Excel_File_With_Options()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var stream = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(stream)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[1].Rows;

                Assert.AreEqual(2, excelData.Tables.Count);
                Assert.AreEqual("Option1", excelRows[0].ItemArray[0]);
                Assert.AreEqual("2", excelRows[0].ItemArray[1]);
                Assert.AreEqual("Option2", excelRows[1].ItemArray[0]);
                Assert.AreEqual("1", excelRows[1].ItemArray[1]);
                Assert.AreEqual(2, excelRows.Count);

                excelReader.Close();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _excelBuilder?.Dispose();
        }

        private Guid MockParticipantsWithOptionsForExport(UserAndOrganizationDTO userAndOrg)
        {
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDTO>
            {
                new EventParticipantDTO
                {
                    FirstName = "Name",
                    LastName = "Surname"
                }
            };

            var options = new List<EventOptionCountDTO>
            {
                new EventOptionCountDTO
                {
                   Option = "Option1",
                   Count = 2
                },
                new EventOptionCountDTO
                {
                   Option = "Option2",
                   Count = 1
                }
            };

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);
            return eventId;
        }

        private Guid MockParticipantsWithoutOptionsForExport(UserAndOrganizationDTO userAndOrg)
        {
            var eventId = Guid.NewGuid();

            var users = new List<EventParticipantDTO>
            {
                new EventParticipantDTO
                {
                    FirstName = "Name",
                    LastName = "Surname"
                }
            };

            // ReSharper disable once CollectionNeverUpdated.Local
            var options = new List<EventOptionCountDTO>();

            _eventParticipationService.GetEventParticipantsAsync(eventId, userAndOrg).Returns(users);
            _eventUtilitiesService.GetEventChosenOptionsAsync(eventId, userAndOrg).Returns(options);
            return eventId;
        }
    }
}
