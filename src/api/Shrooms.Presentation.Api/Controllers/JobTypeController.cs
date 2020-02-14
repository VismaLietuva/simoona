using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.Jobs;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Jobs;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("JobType")]
    public class JobTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IJobService _jobService;

        public JobTypeController(IMapper mapper, IJobService jobService)
        {
            _mapper = mapper;
            _jobService = jobService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        [Route("Types")]
        public async Task<IHttpActionResult> GetJobTypes()
        {
            var jobTypeDto = await _jobService.GetJobTypes(GetUserAndOrganization());
            var jobTypeViewModel = _mapper.Map<IEnumerable<JobTypeDTO>, IEnumerable<JobTypeViewModel>>(jobTypeDto);

            return Ok(jobTypeViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        [Route("Get")]
        public async Task<IHttpActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid request");
            }

            try
            {
                var jobTypeDto = await _jobService.GetJobType(id, GetUserAndOrganization());
                var jobTypeViewModel = _mapper.Map<JobTypeDTO, JobTypeViewModel>(jobTypeDto);

                return Ok(jobTypeViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        public async Task<IHttpActionResult> Create(NewJobTypeViewModel jobTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jobTypeDto = _mapper.Map<NewJobTypeViewModel, JobTypeDTO>(jobTypeViewModel);
            SetOrganizationAndUser(jobTypeDto);

            try
            {
                await _jobService.CreateJobType(jobTypeDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        public async Task<IHttpActionResult> Update(JobTypeViewModel jobTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jobTypeDto = _mapper.Map<JobTypeViewModel, JobTypeDTO>(jobTypeViewModel);
            SetOrganizationAndUser(jobTypeDto);

            try
            {
                await _jobService.UpdateJobType(jobTypeDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                await _jobService.RemoveJobType(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}