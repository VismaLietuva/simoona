using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Employees;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Domain.Services.Employees
{
    public interface IEmployeeListingService
    {
        Task<IPagedList<EmployeeDto>> GetPagedEmployeesAsync(EmployeeListingArgsDto employeeArgsDto, UserAndOrganizationDto userOrg);
    }
}
