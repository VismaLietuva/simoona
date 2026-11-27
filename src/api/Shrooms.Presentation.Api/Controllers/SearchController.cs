using System;
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
        /// Every section's matches for the global search, in one request.
        /// </summary>
        /// <param name="s">What has been typed. Fewer than two characters returns empty groups.</param>
        /// <param name="limit">Most entries per group; capped at 50.</param>
        /// <param name="sections">Comma-separated section names (people, walls, posts, events, kudos, projects, groups, books, polls, videos, serviceRequests) to search only those. Leave out to search every section.</param>
        /// <response code="200">Matches per group. A group the user has no permission for is empty.</response>
        [HttpGet]
        [Route("Quick")]
        [PermissionAnyOfAuthorize(
            BasicPermissions.EmployeeList,
            BasicPermissions.Post,
            BasicPermissions.Event,
            BasicPermissions.Kudos,
            BasicPermissions.Project,
            BasicPermissions.Groups,
            BasicPermissions.Book,
            BasicPermissions.Poll,
            AdministrationPermissions.Poll,
            BasicPermissions.VideoLibrary,
            AdministrationPermissions.VideoLibrary,
            BasicPermissions.ServiceRequest)]
        [ProducesResponseType(typeof(GlobalSearchResultsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Quick(string s = null, int limit = DefaultLimit, string sections = null, CancellationToken cancellationToken = default)
        {
            var only = sections?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return Ok(await _globalSearchService.SearchAsync(s, limit, GetUserAndOrganization(), only, cancellationToken));
        }
    }
}
