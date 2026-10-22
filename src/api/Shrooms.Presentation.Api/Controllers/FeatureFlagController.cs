using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.FeatureFlags;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("FeatureFlag")]
    public class FeatureFlagController : BaseController
    {
        private readonly IFeatureFlagService _featureFlagService;

        public FeatureFlagController(IFeatureFlagService featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        [HttpGet]
        [Route("Enabled")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Enabled()
        {
            return Ok(await _featureFlagService.GetEnabledAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("List")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        [ProducesResponseType(typeof(IEnumerable<FeatureFlagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            return Ok(await _featureFlagService.GetAllAsync(GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public Task<IActionResult> Create([FromBody] NewFeatureFlagDto input)
        {
            return GuardedAsync(async () => Ok(new { Id = await _featureFlagService.CreateAsync(input, GetUserAndOrganization()) }));
        }

        [HttpPut]
        [Route("{id:int}")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public Task<IActionResult> Update(int id, [FromBody] UpdateFeatureFlagDto input)
        {
            return GuardedAsync(async () =>
            {
                await _featureFlagService.UpdateAsync(id, input, GetUserAndOrganization());
                return Ok();
            });
        }

        [HttpDelete]
        [Route("{id:int}")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Organization)]
        public Task<IActionResult> Delete(int id)
        {
            return GuardedAsync(async () =>
            {
                await _featureFlagService.DeleteAsync(id, GetUserAndOrganization());
                return Ok();
            });
        }

        private async Task<IActionResult> GuardedAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (ValidationException e) when (e.ErrorCode == ErrorCodes.DuplicatesIntolerable)
            {
                return Conflict(new { e.ErrorCode, e.ErrorMessage });
            }
            catch (ValidationException e) when (e.ErrorCode == ErrorCodes.ContentDoesNotExist)
            {
                return NotFound();
            }
        }
    }
}
