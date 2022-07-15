using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Extensions;
using Shrooms.Domain.Services.Employees;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Employees;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("EmployeeList")]
    public class EmployeeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeListingService _employeeListingService;

        public EmployeeController(IMapper mapper, IEmployeeListingService employeeListingService)
        {
            _mapper = mapper;
            _employeeListingService = employeeListingService;
        }

        [HttpGet]
        [Route("GetPaged")]
        [PermissionAuthorize(Permission = BasicPermissions.EmployeeList)]
        public async Task<IHttpActionResult> GetPagedEmployees([FromUri] EmployeeListingArgsViewModel employeeArgsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                employeeArgsViewModel ??= new EmployeeListingArgsViewModel();

                var employeeArgsDto = _mapper.Map<EmployeeListingArgsViewModel, EmployeeListingArgsDto>(employeeArgsViewModel);
                var pagedEmployeeDtos = await _employeeListingService.GetPagedEmployeesAsync(employeeArgsDto, GetUserAndOrganization());
                var employeeViewModels = _mapper.Map<IEnumerable<EmployeeDto>, IEnumerable<EmployeeViewModel>>(pagedEmployeeDtos);

                return Ok(pagedEmployeeDtos.ToPagedViewModel(employeeViewModels, employeeArgsViewModel));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
