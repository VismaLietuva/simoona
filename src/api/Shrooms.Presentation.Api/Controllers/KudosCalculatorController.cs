using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("KudosCalculator")]
    public class KudosCalculatorController : BaseController
    {
        private readonly IKudosCalculatorSettingsService _settingsService;

        public KudosCalculatorController(IKudosCalculatorSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        [Route("Settings")]
        [PermissionAuthorize(Permission = BasicPermissions.Kudos)]
        [ProducesResponseType(typeof(KudosCalculatorSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings()
        {
            return Ok(await _settingsService.GetAsync(GetUserAndOrganization()));
        }

        [HttpPut]
        [Route("Settings")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        public async Task<IActionResult> UpdateSettings([FromBody] KudosCalculatorSettingsDto input)
        {
            try
            {
                await _settingsService.UpdateAsync(input, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
