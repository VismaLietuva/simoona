using System;
using System.Linq.Expressions;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public interface IServiceRequestExportService
    {
        byte[] ExportToExcel(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
