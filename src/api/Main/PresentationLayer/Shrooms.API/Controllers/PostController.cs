using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.API.Hubs;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.WebViewModels.Models.Notifications;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.API.Controllers
{
    [Authorize]
    [RoutePrefix("Post")]
    public class PostController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public PostController(IMapper mapper, IWallService wallService, IPostService postService, INotificationService notificationService, IUserService userService)
        {
            _mapper = mapper;
            _wallService = wallService;
            _postService = postService;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> GetPost(int postId)
        {
            try
            {
                var userAndOrg = GetUserAndOrganization();
                var wallPost = await _wallService.GetWallPost(userAndOrg, postId);

                var mappedPost = _mapper.Map<WallPostViewModel>(wallPost);
                return Ok(mappedPost);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> CreatePost(CreateWallPostViewModel wallPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postModel = _mapper.Map<CreateWallPostViewModel, NewPostDTO>(wallPostViewModel);
            SetOrganizationAndUser(postModel);

            try
            {
                var createdPost = _postService.CreateNewPost(postModel);

                var membersToNotify = _userService.GetWallUserAppNotificationEnabledIds(postModel.UserId, postModel.WallId);

                var notificationDto = await _notificationService.CreateForPost(GetUserAndOrganization(), createdPost, postModel.WallId, membersToNotify);

                NotificationHub.SendNotificationToParticularUsers(_mapper.Map<NotificationViewModel>(notificationDto), GetUserAndOrganizationHub(), membersToNotify);
                NotificationHub.SendWallNotification(wallPostViewModel.WallId, membersToNotify, createdPost.WallType, GetUserAndOrganizationHub());

                return Ok(_mapper.Map<WallPostViewModel>(createdPost));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult EditPost(EditPostViewModel editedPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var editPostDto = _mapper.Map<EditPostDTO>(editedPost);
            SetOrganizationAndUser(editPostDto);
            try
            {
                _postService.EditPost(editPostDto);
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
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult DeletePost(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                _postService.DeleteWallPost(id, userAndOrg);
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
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult HidePost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                _postService.HideWallPost(post.Id, userAndOrg);
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
                _postService.ToggleLike(id, userAndOrg);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}