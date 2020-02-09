using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.BackgroundWorkers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.WebViewModels.Models.Wall.Posts.Comments;

namespace Shrooms.API.Controllers
{
    [RoutePrefix("Comment")]
    public class CommentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;
        private readonly IWallService _wallService;
        private readonly IAsyncRunner _asyncRunner;

        public CommentController(IMapper mapper, ICommentService commentService, IWallService wallService, IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _commentService = commentService;
            _wallService = wallService;
            _asyncRunner = asyncRunner;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Comment)]
        public IHttpActionResult CreateComment(NewCommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var commentDto = _mapper.Map<NewCommentViewModel, NewCommentDTO>(comment);
            SetOrganizationAndUser(commentDto);
            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                var commentCreatedDto = _commentService.CreateComment(commentDto);
                _asyncRunner.Run<NewCommentNotifier>(notif =>
                {
                    notif.Notify(commentCreatedDto, userHubDto);
                }, GetOrganizationName());

                return Ok(new { commentCreatedDto.CommentId });
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Comment, BasicPermissions.EventWall)]
        public IHttpActionResult EditComment(EditCommentViewModel commentViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var editCommentDto = _mapper.Map<EditCommentViewModel, EditCommentDTO>(commentViewModel);
            SetOrganizationAndUser(editCommentDto);

            try
            {
                _commentService.EditComment(editCommentDto);
                return Ok();
            }
            catch (UnauthorizedException)
            {
                return BadRequest();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Comment, BasicPermissions.EventWall)]
        public IHttpActionResult DeleteComment(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _commentService.DeleteComment(id, GetUserAndOrganization());
                return Ok();
            }
            catch (UnauthorizedException)
            {
                return BadRequest();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Hide")]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Comment, BasicPermissions.EventWall)]
        public IHttpActionResult HideComment(HideCommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _commentService.HideComment(comment.Id, GetUserAndOrganization());
                return Ok();
            }
            catch (UnauthorizedException)
            {
                return BadRequest();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Like")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult ToggleLike(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                _commentService.ToggleLike(id, userAndOrg);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("CreateEventComment")]
        [PermissionAuthorize(Permission = BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> CreateEventComment(NewCommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();

            var wallPost = await _wallService.GetWallPost(userAndOrg, comment.PostId);
            if (wallPost.WallType != WallType.Events)
            {
                return Forbidden();
            }

            var commentDto = _mapper.Map<NewCommentViewModel, NewCommentDTO>(comment);
            SetOrganizationAndUser(commentDto);

            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                var commentCreatedDto = _commentService.CreateComment(commentDto);
                _asyncRunner.Run<NewCommentNotifier>(notif =>
                {
                    notif.Notify(commentCreatedDto, userHubDto);
                }, GetOrganizationName());

                return Ok(new { commentCreatedDto.CommentId });
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}