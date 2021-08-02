using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Domain.Services.Committees;
using Shrooms.Premium.Presentation.WebViewModels.Committees;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

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
        public override Task<IEnumerable<CommitteeViewModel>> GetAllAsync(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAllAsync(maxResults, orderBy, includeProperties);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override Task<HttpResponseMessage> Delete(int id)
        {
            return base.Delete(id);
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override async Task<HttpResponseMessage> Put(CommitteePostViewModel postViewModel)
        {
            var dto = _mapper.Map<CommitteePostViewModel, CommitteePostDTO>(postViewModel);
            try
            {
                if (dto.Name != null && dto.Description != null)
                {
                    await _committeesService.PutCommitteeAsync(dto);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage KudosCommittee()
        {
            var kudosCommittee = _committeesService.GetKudosCommitteeAsync();

            return Request.CreateResponse(HttpStatusCode.OK, kudosCommittee);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage KudosCommitteeId()
        {
            var id = _committeesService.GetKudosCommitteeIdAsync();

            return Request.CreateResponse(HttpStatusCode.OK, new { id });
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override async Task<HttpResponseMessage> Post(CommitteePostViewModel postViewModel)
        {
            var dto = _mapper.Map<CommitteePostViewModel, CommitteePostDTO>(postViewModel);
            try
            {
                if (dto.Name != null && dto.Description != null)
                {
                    await _committeesService.PostCommitteeAsync(dto);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public async Task<HttpResponseMessage> PostSuggestion(CommitteeSuggestionPostViewModel postViewModel)
        {
            if (string.IsNullOrWhiteSpace(postViewModel.Title))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (string.IsNullOrWhiteSpace(postViewModel.Description))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (postViewModel.CommitteeId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }

            var modelDTO = _mapper.Map<CommitteeSuggestionPostDTO>(postViewModel);

            try
            {
                await _committeesService.PostSuggestionAsync(modelDTO, GetUserAndOrganization().UserId);
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public HttpResponseMessage DeleteSuggestion(int committeeId, int suggestionId)
        {
            var userAndOrg = GetUserAndOrganization();
            try
            {
                _committeesService.DeleteCommitteeSuggestion(committeeId, suggestionId, userAndOrg);
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage GetSuggestions(int id)
        {
            if (id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }
            var suggestions = _committeesService.GetCommitteeSuggestions(id);

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<IEnumerable<CommitteeSuggestionViewModel>>(suggestions));
        }
    }
}