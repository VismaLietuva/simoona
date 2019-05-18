using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.API.Hubs;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Notifications;
using Shrooms.Host.Contracts.Constants;
using Shrooms.WebViewModels.Models.Notifications;
using Shrooms.WebViewModels.Models.Wall.Posts.Comments;

namespace Shrooms.API.Controllers
{
    [RoutePrefix("Comment")]
    public class CommentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;

        public CommentController(IMapper mapper, IWallService wallService, ICommentService commentService, INotificationService notificationService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _commentService = commentService;
            _notificationService = notificationService;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Comment)]
        public async Task<IHttpActionResult> CreateComment(NewCommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var commentDto = _mapper.Map<NewCommentViewModel, NewCommentDTO>(comment);
            SetOrganizationAndUser(commentDto);

            try
            {
                var commentCreatedDto = _commentService.CreateComment(commentDto);

                var membersToNotify = _wallService.GetWallMembersIds(commentCreatedDto.WallId, commentDto);
                NotificationHub.SendWallNotification(commentCreatedDto.WallId, membersToNotify, commentCreatedDto.WallType, GetUserAndOrganizationHub());

                var commentsAuthorsToNotify = _commentService.GetCommentsAuthorsToNotify(
                                                    commentDto.PostId,
                                                    new List<string>() { commentCreatedDto.CommentCreator });

                if (commentCreatedDto.PostCreator != commentCreatedDto.CommentCreator && _commentService.IsPostAuthorAppNotificationsEnabled(commentCreatedDto.PostCreator))
                {
                    var notificationAuthorDto = await _notificationService.CreateForComment(GetUserAndOrganization(), commentCreatedDto, NotificationType.WallComment, new List<string> { commentCreatedDto.PostCreator });
                    if (notificationAuthorDto != null)
                    {
                        NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationAuthorDto), GetUserAndOrganizationHub(), new List<string>() { commentCreatedDto.PostCreator });
                    }

                    commentsAuthorsToNotify.Remove(commentCreatedDto.PostCreator);
                }

                if (commentsAuthorsToNotify.Count > 0)
                {
                    var notificationDto = await _notificationService.CreateForComment(GetUserAndOrganization(), commentCreatedDto, NotificationType.FollowingComment, commentsAuthorsToNotify);
                    if (notificationDto != null)
                    {
                        NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationDto), GetUserAndOrganizationHub(), commentsAuthorsToNotify);
                    }
                }

                return Ok(new { commentCreatedDto.CommentId });
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = BasicPermissions.Comment)]
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
        [PermissionAuthorize(Permission = BasicPermissions.Comment)]
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
        [PermissionAuthorize(Permission = BasicPermissions.Comment)]
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