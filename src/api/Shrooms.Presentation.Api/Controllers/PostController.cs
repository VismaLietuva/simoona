using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Presentation.Api.BackgroundWorkers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Post")]
    public class PostController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IWallService _wallService;
        private readonly IPostService _postService;
        private readonly IPermissionService _permissionService;
        private readonly IAsyncRunner _asyncRunner;

        public PostController(IMapper mapper, IWallService wallService, IPostService postService, IPermissionService permissionService, IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _wallService = wallService;
            _postService = postService;
            _permissionService = permissionService;
            _asyncRunner = asyncRunner;
        }

        [HttpGet]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> GetPost(int postId)
        {
            try
            {
                var userAndOrg = GetUserAndOrganization();
                var wallPost = await _wallService.GetWallPost(userAndOrg, postId);

                if (!_permissionService.UserHasPermission(userAndOrg, BasicPermissions.Post) && wallPost.WallType != WallType.Events)
                {
                    return Forbidden();
                }

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
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> CreatePost(CreateWallPostViewModel wallPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();

            if (!_permissionService.UserHasPermission(userAndOrg, BasicPermissions.Post))
            {
                var wall = await _wallService.GetWall(wallPostViewModel.WallId, userAndOrg);
                if (wall.Type != WallType.Events)
                {
                    return Forbidden();
                }
            }

            var postModel = _mapper.Map<CreateWallPostViewModel, NewPostDTO>(wallPostViewModel);
            SetOrganizationAndUser(postModel);
            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                var createdPost = _postService.CreateNewPost(postModel);
                _asyncRunner.Run<NewPostNotifier>(notif =>
                {
                    notif.Notify(createdPost, userHubDto);
                }, GetOrganizationName());

                return Ok(_mapper.Map<WallPostViewModel>(createdPost));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
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
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
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
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
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
        [PermissionAnyOfAuthorizeAttribute(BasicPermissions.Post, BasicPermissions.EventWall)]
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

        [HttpPut]
        [Route("Watch")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult WatchPost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                _postService.ToggleWatch(post.Id, userAndOrg, true);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [Route("Unwatch")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult UnwatchPost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                _postService.ToggleWatch(post.Id, userAndOrg, false);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            return Ok();
        }
    }
}