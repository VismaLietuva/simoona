using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimeReportService
    {
        Task<TimeReportDto> GetReportAsync(string month, UserAndOrganizationDto userOrg);
    }
}
