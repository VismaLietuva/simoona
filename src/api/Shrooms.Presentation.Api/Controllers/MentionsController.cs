using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;
using Shrooms.Domain.Services.Wall.Mentions;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Wall.Mentions;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Mentions")]
    public class MentionsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IMentionSearchService _mentionSearchService;

        public MentionsController(IMapper mapper, IMentionSearchService mentionSearchService)
        {
            _mapper = mapper;
            _mentionSearchService = mentionSearchService;
        }

        /// <summary>
        /// People and taggable groups for the composer's '@' list.
        /// </summary>
        /// <param name="s">What has been typed after the '@'. Empty returns the first entries.</param>
        /// <response code="200">Matching people and groups</response>
        [HttpGet]
        [Route("Search")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Comment, BasicPermissions.Event)]
        [ProducesResponseType(typeof(MentionSuggestionsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(string s = null)
        {
            var suggestions = await _mentionSearchService.SearchAsync(s, GetUserAndOrganization());

            return Ok(_mapper.Map<MentionSuggestionsDto, MentionSuggestionsViewModel>(suggestions));
        }
    }
}
