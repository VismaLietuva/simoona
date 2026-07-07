using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.Jobs;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.Jobs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("JobType")]
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
        [ProducesResponseType(typeof(IEnumerable<JobTypeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobTypes()
        {
            var jobTypeDto = await _jobService.GetJobTypesAsync(GetUserAndOrganization());
            var jobTypeViewModel = _mapper.Map<IEnumerable<JobTypeDto>, IEnumerable<JobTypeViewModel>>(jobTypeDto);

            return Ok(jobTypeViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Job)]
        [Route("Get")]
        [ProducesResponseType(typeof(JobTypeViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid request");
            }

            try
            {
                var jobTypeDto = await _jobService.GetJobTypeAsync(id, GetUserAndOrganization());
                var jobTypeViewModel = _mapper.Map<JobTypeDto, JobTypeViewModel>(jobTypeDto);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(NewJobTypeViewModel jobTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jobTypeDto = _mapper.Map<NewJobTypeViewModel, JobTypeDto>(jobTypeViewModel);
            SetOrganizationAndUser(jobTypeDto);

            try
            {
                await _jobService.CreateJobTypeAsync(jobTypeDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(JobTypeViewModel jobTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jobTypeDto = _mapper.Map<JobTypeViewModel, JobTypeDto>(jobTypeViewModel);
            SetOrganizationAndUser(jobTypeDto);

            try
            {
                await _jobService.UpdateJobTypeAsync(jobTypeDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                await _jobService.RemoveJobTypeAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
