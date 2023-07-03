using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Excel;
using NSubstitute;
using NUnit.Framework;
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

            var content = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
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
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithoutOptionsForExport(userAndOrg);

            var content = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
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
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithOptionsForExport(userAndOrg);

            var content = await _eventExportService.ExportOptionsAndParticipantsAsync(guid, userAndOrg);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
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
