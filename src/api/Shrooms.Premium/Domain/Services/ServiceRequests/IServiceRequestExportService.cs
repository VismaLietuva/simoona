using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.ServiceRequests
{
    public interface IServiceRequestExportService
    {
        Task<ByteArrayContent> ExportToExcelAsync(UserAndOrganizationDto userAndOrg, Expression<Func<ServiceRequest, bool>> filter);
    }
}
