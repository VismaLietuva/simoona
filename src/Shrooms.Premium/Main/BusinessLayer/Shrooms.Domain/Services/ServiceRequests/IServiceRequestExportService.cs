namespace Shrooms.Domain.Services.ServiceRequests.Export
{
    using System;
    using System.Linq.Expressions;
    using Shrooms.DataTransferObjects.Models;
    using Shrooms.EntityModels.Models;

    public interface IServiceRequestExportService
    {
        byte[] ExportToExcel(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
