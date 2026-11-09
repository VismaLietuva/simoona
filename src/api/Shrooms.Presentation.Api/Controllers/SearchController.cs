using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.DataTransferObjects.Models.Search;
using Shrooms.Domain.Services.Search;
using Shrooms.Presentation.Common.Controllers;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Search")]
    public class SearchController : BaseController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // Each section's own permission is applied inside the search, so no single
        // permission gates the endpoint; the GlobalSearch feature flag does.
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Search(string query, string context = null, CancellationToken cancellationToken = default)
        {
            var userOrg = GetUserAndOrganization();
            if (!await _searchService.IsEnabledAsync(userOrg))
            {
                return NotFound();
            }

            return Ok(await _searchService.SearchAsync(userOrg, query, context, cancellationToken));
        }
    }
}
