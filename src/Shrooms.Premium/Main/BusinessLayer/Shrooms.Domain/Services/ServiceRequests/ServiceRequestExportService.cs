using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Domain.Services.ServiceRequests.Export
{
    public class ServiceRequestExportService : IServiceRequestExportService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<ServiceRequest> _serviceRequestsDbSet;
        private readonly IExcelBuilder _excelBuilder;

        public ServiceRequestExportService(IUnitOfWork2 uow, IExcelBuilder excelBuilder)
        {
            _uow = uow;
            _serviceRequestsDbSet = _uow.GetDbSet<ServiceRequest>();
            _excelBuilder = excelBuilder;
        }

        public byte[] ExportToExcel(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter)
        {
            IQueryable<ServiceRequest> query = _serviceRequestsDbSet
                .Include(x => x.Status)
                .Include(x => x.Priority)
                .Include(x => x.Employee)
                .Where(x => x.OrganizationId == userAndOrg.OrganizationId);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var serviceRequests = query
                .OrderByDescending(x => x.Created)
                .AsEnumerable()
                .Select(x => new List<object>
                {
                    x.Title,
                    x.Description,
                    x.CategoryName,
                    x.Status?.Title,
                    x.Priority?.Title,
                    x.Employee?.FullName,
                    x.KudosAmmount,
                    x.KudosShopItem?.Name,
                    x.Created,
                    x.Modified
                });

            var header = new List<string>
            {
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameTitle,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameDescription,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameCategory,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameStatus,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNamePriority,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameUser,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameKudosAmount,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameKudosShopItem,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameDate,
                Resources.Models.ServiceRequest.ServiceRequest.ExportColumnNameModified
            };

            _excelBuilder.AddNewWorksheet(
                ConstBusinessLayer.ServiceRequestsExcelSheetName,
                header,
                serviceRequests);

            return _excelBuilder.GenerateByteArray();
        }
    }
}
