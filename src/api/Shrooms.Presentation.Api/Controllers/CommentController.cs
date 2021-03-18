﻿using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Presentation.Api.BackgroundWorkers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments;

namespace Shrooms.Presentation.Api.Controllers
{
    [RoutePrefix("Comment")]
    public class CommentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;
        private readonly IWallService _wallService;
        private readonly IPermissionService _permissionService;
        private readonly IAsyncRunner _asyncRunner;

        public CommentController(IMapper mapper, ICommentService commentService, IWallService wallService, IPermissionService permissionService, IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _commentService = commentService;
            _wallService = wallService;
            _permissionService = permissionService;
            _asyncRunner = asyncRunner;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Comment, BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> CreateComment(NewCommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();

            var wallPost = await _wallService.GetWallPost(userAndOrg, comment.PostId);
            if (!_permissionService.UserHasPermission(userAndOrg, BasicPermissions.Comment) && wallPost.WallType != WallType.Events)
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
    }
}