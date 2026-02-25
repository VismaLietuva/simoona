using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Domain.Services.Committees;
using Shrooms.Premium.Presentation.WebViewModels.Committees;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    public class CommitteesController : AbstractWebApiController<Committee, CommitteeViewModel, CommitteePostViewModel>
    {
        private readonly ICommitteesService _committeesService;

        public CommitteesController(IMapper mapper, IUnitOfWork unitOfWork, ICommitteesService committeesService)
            : base(mapper, unitOfWork, "Created")
        {
            _committeesService = committeesService;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public override async Task<IEnumerable<CommitteeViewModel>> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return await base.GetAll(maxResults, orderBy, includeProperties);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override async Task<IActionResult> Delete(int id)
        {
            return await base.Delete(id);
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override async Task<IActionResult> Put(CommitteePostViewModel postViewModel)
        {
            var dto = _mapper.Map<CommitteePostViewModel, CommitteePostDto>(postViewModel);
            try
            {
                if (dto.Name != null && dto.Description != null)
                {
                    await _committeesService.PutCommitteeAsync(dto);
                }
                else
                {
                    return NotFound(new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return BadRequest(new[] { ex.Message });
            }
            return StatusCode(201);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public async Task<IActionResult> KudosCommittee()
        {
            var kudosCommittee = await _committeesService.GetKudosCommitteeAsync();

            return Ok(kudosCommittee);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public async Task<IActionResult> KudosCommitteeId()
        {
            var id = await _committeesService.GetKudosCommitteeIdAsync();

            return Ok(new { id });
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override async Task<IActionResult> Post(CommitteePostViewModel postViewModel)
        {
            var dto = _mapper.Map<CommitteePostViewModel, CommitteePostDto>(postViewModel);
            try
            {
                if (dto.Name != null && dto.Description != null)
                {
                    await _committeesService.PostCommitteeAsync(dto);
                }
                else
                {
                    return NotFound(new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return BadRequest(new[] { ex.Message });
            }
            return StatusCode(201);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public async Task<IActionResult> PostSuggestion(CommitteeSuggestionPostViewModel postViewModel)
        {
            if (string.IsNullOrWhiteSpace(postViewModel.Title))
            {
                return NotFound(new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (string.IsNullOrWhiteSpace(postViewModel.Description))
            {
                return NotFound(new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (postViewModel.CommitteeId == 0)
            {
                return NotFound(new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }

            var dto = _mapper.Map<CommitteeSuggestionPostDto>(postViewModel);

            try
            {
                await _committeesService.PostSuggestionAsync(dto, GetUserAndOrganization().UserId);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new[] { ex.Message });
            }
            return StatusCode(201);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public async Task<IActionResult> DeleteSuggestion(int committeeId, int suggestionId)
        {
            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _committeesService.DeleteCommitteeSuggestionAsync(committeeId, suggestionId, userAndOrg);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new[] { ex.Message });
            }

            return Ok();
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public async Task<IActionResult> GetSuggestions(int id)
        {
            if (id == 0)
            {
                return NotFound(new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }
            var suggestions = await _committeesService.GetCommitteeSuggestionsAsync(id);

            return Ok(_mapper.Map<IEnumerable<CommitteeSuggestionViewModel>>(suggestions));
        }
    }
}
