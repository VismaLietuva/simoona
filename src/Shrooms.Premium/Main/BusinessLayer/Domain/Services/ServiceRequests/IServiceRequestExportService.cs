using System;
using System.Linq.Expressions;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.ServiceRequests
{
    public interface IServiceRequestExportService
    {
        byte[] ExportToExcel(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
