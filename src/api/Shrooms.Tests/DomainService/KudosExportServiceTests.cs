using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    public class KudosExportServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<KudosLog> _kudosDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private IKudosExportService _kudosExportService;
        private ExcelBuilderFactory _excelBuilder;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _kudosDbSet = Substitute.For<DbSet<KudosLog>, IQueryable<KudosLog>, IAsyncEnumerable<KudosLog>>();
            _kudosDbSet.SetDbSetDataForAsync(MockKudos());
            _uow.GetDbSet<KudosLog>().Returns(_kudosDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _excelBuilder = new ExcelBuilderFactory();

            _kudosExportService = new KudosExportService(
                _uow,
                _excelBuilder,
                Substitute.For<ISystemClock>());
        }

        [Test]
        public async Task Kudos_Should_Return_Excel_File()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = null,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var export = await _kudosExportService.ExportToExcelAsync(filter);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;
                var excelColumns = excelData.Tables[0].Columns;

                Assert.That(excelColumns[0].ColumnName, Is.EqualTo(Resources.Models.Kudos.Kudos.ExportColumnSender));
                Assert.That(excelColumns[1].ColumnName, Is.EqualTo(Resources.Models.Kudos.Kudos.ExportColumnReceiver));
                Assert.That(excelRows[0].ItemArray[0], Is.EqualTo("name surname"));
                Assert.That(excelRows[0].ItemArray[1], Is.EqualTo("name surname"));
                Assert.That(excelRows[1].ItemArray[0], Is.EqualTo("name2 surname2"));
                Assert.That(excelRows[1].ItemArray[1], Is.EqualTo("name surname"));
                Assert.That(excelRows.Count, Is.EqualTo(4));

                excelReader.Close();
            }
        }

        [Test]
        public async Task Kudos_Should_Return_Filtered_Excel_File()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = "testUserId3",
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var export = await _kudosExportService.ExportToExcelAsync(filter);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;

                Assert.That(excelRows[0].ItemArray[0], Is.EqualTo("name5 surname5"));
                Assert.That(excelRows[0].ItemArray[1], Is.EqualTo("name3 surname3"));
                Assert.That(excelRows.Count, Is.EqualTo(1));

                excelReader.Close();
            }
        }

        [Test]
        public async Task Kudos_Should_Return_Type_Filtered_Excel_File()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = null,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                FilteringType = "Other",
                SortBy = "Created",
                SortOrder = "desc"
            };

            var export = await _kudosExportService.ExportToExcelAsync(filter);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;

                Assert.That(excelRows.Count, Is.EqualTo(2));
                Assert.That(excelRows[0].ItemArray[2], Is.EqualTo("Other"));
                Assert.That(excelRows[1].ItemArray[2], Is.EqualTo("Other"));

                excelReader.Close();
            }
        }

        [Test]
        public async Task Kudos_Should_Return_Every_Type_When_Filtering_Type_Is_All()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = null,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                FilteringType = BusinessLayerConstants.KudosFilteringTypeAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var export = await _kudosExportService.ExportToExcelAsync(filter);

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(export.Content)))
            {
                var excelData = excelReader.AsDataSet(new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } });
                var excelRows = excelData.Tables[0].Rows;

                Assert.That(excelRows.Count, Is.EqualTo(4));

                excelReader.Close();
            }
        }

        private IQueryable<KudosLog> MockKudos()
        {
            return new List<KudosLog>
            {
                new()
                {
                    Status = KudosStatus.Pending,
                    Id = 1,
                    KudosTypeName = "Other",
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
                {
                    Status = KudosStatus.Pending,
                    Id = 2,
                    KudosTypeName = "Send",
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId2"
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 3,
                    KudosTypeName = "Other",
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    KudosTypeName = "Welcome",
                    EmployeeId = "testUserId3",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name3",
                        LastName = "surname3"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId5"
                }
            }.AsQueryable();
        }

        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new()
                {
                    Id = "testUserId",
                    FirstName = "name",
                    LastName = "surname"
                },
                new()
                {
                    Id = "testUserId2",
                    FirstName = "name2",
                    LastName = "surname2"
                },
                new()
                {
                    Id = "testUserId3",
                    FirstName = "name3",
                    LastName = "surname3"
                },
                new()
                {
                    Id = "testUserId4",
                    FirstName = "name4",
                    LastName = "surname4"
                },
                new()
                {
                    Id = "testUserId5",
                    FirstName = "name5",
                    LastName = "surname5"
                }
            }.AsQueryable();
        }
    }
}
