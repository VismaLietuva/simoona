using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public class ServiceRequestExportService : IServiceRequestExportService
    {
        private readonly IDbSet<ServiceRequest> _serviceRequestsDbSet;
        private readonly IExcelBuilder _excelBuilder;

        public ServiceRequestExportService(IUnitOfWork2 uow, IExcelBuilder excelBuilder)
        {
            _serviceRequestsDbSet = uow.GetDbSet<ServiceRequest>();
            _excelBuilder = excelBuilder;
        }

        public async Task<byte[]> ExportToExcelAsync(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter)
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

            var serviceRequests = (await query.OrderByDescending(x => x.Created).ToListAsync())
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

            _excelBuilder.AddNewWorksheet(ServiceRequestConstants.ServiceRequestsExcelSheetName, header, serviceRequests);

            return _excelBuilder.GenerateByteArray();
        }
    }
}
