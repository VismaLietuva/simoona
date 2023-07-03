using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Excel;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Premium.Domain.Services.ServiceRequests;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    public class ServiceRequestExportServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<ServiceRequest> _serviceRequestsDbSet;
        private IServiceRequestExportService _serviceRequestExportService;
        private IExcelBuilderFactory _excelBuilder;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _serviceRequestsDbSet = Substitute.For<DbSet<ServiceRequest>, IQueryable<ServiceRequest>, IDbAsyncEnumerable<ServiceRequest>>();
            _uow.GetDbSet<ServiceRequest>().Returns(_serviceRequestsDbSet);

            _excelBuilder = new ExcelBuilderFactory();

            _serviceRequestExportService = new ServiceRequestExportService(_uow, _excelBuilder);
        }

        [Test]
        public async Task ServiceRequests_Should_Return_Excel_File()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            MockServiceRequests();

            var content = await _serviceRequestExportService.ExportToExcelAsync(userAndOrg, null);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[0].Rows;
                var excelColumns = excelData.Tables[0].Columns;

                Assert.AreEqual(Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameTitle, excelColumns[0].ColumnName);
                Assert.AreEqual(Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameDescription, excelColumns[1].ColumnName);
                Assert.AreEqual("Need 1", excelRows[0].ItemArray[0]);
                Assert.AreEqual("Description 1", excelRows[0].ItemArray[1]);
                Assert.AreEqual(3, excelRows.Count);

                excelReader.Close();
            }
        }

        [Test]
        public async Task ServiceRequests_Should_Return_Excel_File_With_Filtered_Categories()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            MockServiceRequests();

            Expression<Func<ServiceRequest, bool>> filter = f => f.CategoryName == "Hardware";

            var content = await _serviceRequestExportService.ExportToExcelAsync(userAndOrg, filter);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[0].Rows;

                Assert.AreEqual("Need 2", excelRows[0].ItemArray[0]);
                Assert.AreEqual("Description 2", excelRows[0].ItemArray[1]);
                Assert.AreEqual(1, excelRows.Count);

                excelReader.Close();
            }
        }

        private void MockServiceRequests()
        {
            var serviceRequests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    Id = 1,
                    Title = "Need 1",
                    Description = "Description 1",
                    OrganizationId = 2
                },
                new ServiceRequest
                {
                    Id = 2,
                    Title = "Need 2",
                    Description = "Description 2",
                    CategoryName = "Hardware",
                    OrganizationId = 2
                },
                new ServiceRequest
                {
                    Id = 3,
                    Title = "Need 3",
                    Description = "Description 3",
                    CategoryName = "Office stuff",
                    OrganizationId = 2
                },
                new ServiceRequest
                {
                    Id = 4,
                    Title = "Org 1 Need 1",
                    Description = "Org 1 Description 1",
                    CategoryName = "Office stuff",
                    OrganizationId = 1
                }
            };
            _serviceRequestsDbSet.SetDbSetDataForAsync(serviceRequests);
        }
    }
}
