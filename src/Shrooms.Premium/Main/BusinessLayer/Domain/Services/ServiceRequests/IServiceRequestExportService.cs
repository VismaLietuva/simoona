using System;
using System.Linq.Expressions;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.ServiceRequests
{
    public interface IServiceRequestExportService
    {
        byte[] ExportToExcel(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
