using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Search;
using Shrooms.Domain.Services.Search;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Controllers;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Search")]
    public class SearchController : BaseController
    {
        private const int DefaultLimit = 8;

        private readonly IGlobalSearchService _globalSearchService;

        public SearchController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        /// <summary>
        /// People, walls, events and posts for the global search, in one request.
        /// </summary>
        /// <param name="s">What has been typed. Fewer than two characters returns empty groups.</param>
        /// <param name="limit">Most entries per group; capped at 50.</param>
        /// <response code="200">Matches per group. A group the user has no permission for is empty.</response>
        [HttpGet]
        [Route("Quick")]
        [PermissionAnyOfAuthorize(BasicPermissions.EmployeeList, BasicPermissions.Post, BasicPermissions.Event)]
        [ProducesResponseType(typeof(GlobalSearchResultsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Quick(string s = null, int limit = DefaultLimit, CancellationToken cancellationToken = default)
        {
            return Ok(await _globalSearchService.SearchAsync(s, limit, GetUserAndOrganization(), cancellationToken));
        }
    }
}
