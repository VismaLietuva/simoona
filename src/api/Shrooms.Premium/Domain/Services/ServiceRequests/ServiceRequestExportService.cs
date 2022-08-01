using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Premium.Constants;
using Shrooms.Contracts.Constants;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public class ServiceRequestExportService : IServiceRequestExportService
    {
        private readonly IDbSet<ServiceRequest> _serviceRequestsDbSet;
        private readonly IExcelBuilderFactory _excelBuilderFactory;

        public ServiceRequestExportService(IUnitOfWork2 uow, IExcelBuilderFactory excelBuilderFactory)
        {
            _serviceRequestsDbSet = uow.GetDbSet<ServiceRequest>();
            _excelBuilderFactory = excelBuilderFactory;
        }

        public async Task<ByteArrayContent> ExportToExcelAsync(UserAndOrganizationDto userAndOrg, Expression<Func<ServiceRequest, bool>> filter)
        {
            var query = _serviceRequestsDbSet
                .Include(x => x.Status)
                .Include(x => x.Priority)
                .Include(x => x.Employee)
                .Where(x => x.OrganizationId == userAndOrg.OrganizationId);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var serviceRequests = await query
                .OrderByDescending(x => x.Created)
                .ToListAsync();

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(ServiceRequestConstants.ServiceRequestsExcelSheetName)
                .AddHeader(
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameTitle,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameDescription,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameCategory,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameStatus,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNamePriority,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameUser,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameKudosAmount,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameKudosShopItem,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameDate,
                    Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameModified)
                .AddRows(serviceRequests.AsQueryable(), MapServiceRequestToExcelRow());

            return new ByteArrayContent(excelBuilder.Build());
        }

        private static Expression<Func<ServiceRequest, IExcelRow>> MapServiceRequestToExcelRow()
        {
            return serviceRequest => new ExcelRow
            {
                new ExcelColumn
                {
                    Value = serviceRequest.Title
                },

                new ExcelColumn
                {
                    Value = serviceRequest.Description
                },

                new ExcelColumn
                {
                    Value = serviceRequest.CategoryName
                },

                new ExcelColumn
                {
                    Value = serviceRequest.Status != null ? serviceRequest.Status.Title : string.Empty
                },

                new ExcelColumn
                {
                    Value = serviceRequest.Priority != null ? serviceRequest.Priority.Title : string.Empty
                },

                new ExcelColumn
                {
                    Value = serviceRequest.Employee != null ? serviceRequest.Employee.FullName : string.Empty
                },

                new ExcelColumn
                {
                    Value = serviceRequest.KudosAmmount,
                    Format = ExcelWorksheetBuilderConstants.NumberFormat
                },

                new ExcelColumn
                {
                    Value = serviceRequest.KudosShopItem != null ? serviceRequest.KudosShopItem.Name : string.Empty
                },

                new ExcelColumn
                {
                    Value = serviceRequest.Created,
                    Format = ExcelWorksheetBuilderConstants.DateFormat
                },
                
                new ExcelColumn
                {
                    Value = serviceRequest.Modified,
                    Format = ExcelWorksheetBuilderConstants.DateFormat
                }
            };
        }
    }
}
