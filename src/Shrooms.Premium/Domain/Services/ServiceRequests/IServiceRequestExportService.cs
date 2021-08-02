using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public interface IServiceRequestExportService
    {
        Task<byte[]> ExportToExcelAsync(UserAndOrganizationDTO userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
