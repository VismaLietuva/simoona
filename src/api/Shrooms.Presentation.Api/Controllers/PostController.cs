using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Wall.Likes;
using Shrooms.Contracts.ViewModels.Wall.Posts;
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
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IHttpActionResult> GetPost(int postId)
        {
            try
            {
                var userAndOrg = GetUserAndOrganization();
                var wallPost = await _wallService.GetWallPostAsync(userAndOrg, postId);

                if (!await _permissionService.UserHasPermissionAsync(userAndOrg, BasicPermissions.Post) && wallPost.WallType != WallType.Events)
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
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IHttpActionResult> CreatePost(CreateWallPostViewModel wallPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            var postModel = _mapper.Map<CreateWallPostViewModel, NewPostDto>(wallPostViewModel);

            if (!await _permissionService.UserHasPermissionAsync(userAndOrg, BasicPermissions.Post))
            {
                var wall = await _wallService.GetWallAsync(postModel.WallId, userAndOrg);
                if (wall.Type != WallType.Events)
                {
                    return Forbidden();
                }
            }

            SetOrganizationAndUser(postModel);
            var userHubDto = GetUserAndOrganizationHub();

            try
            {
                var createdPost = await _postService.CreateNewPostAsync(postModel);

                _asyncRunner.Run<PostNotifier>(async notifier => await notifier.NotifyAboutNewPostAsync(createdPost, userHubDto), GetOrganizationName());

                return Ok(_mapper.Map<WallPostViewModel>(createdPost));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IHttpActionResult> EditPost(EditPostViewModel editedPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var editPostDto = _mapper.Map<EditPostDto>(editedPost);

            SetOrganizationAndUser(editPostDto);

            try
            {
                await _postService.EditPostAsync(editPostDto);
                
                _asyncRunner.Run<PostNotifier>(async notifier =>
                    await notifier.NotifyUpdatedPostMentionsAsync(editPostDto), GetOrganizationName());

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
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IHttpActionResult> DeletePost(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _postService.DeleteWallPostAsync(id, userAndOrg);
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
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IHttpActionResult> HidePost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _postService.HideWallPostAsync(post.Id, userAndOrg);
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
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)] // TODO:... make event only work?
        public async Task<IHttpActionResult> ToggleLike(AddLikeViewModel addLikeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            var addLikeDto = _mapper.Map<AddLikeDto>(addLikeViewModel);

            try
            {
                await _postService.ToggleLikeAsync(addLikeDto, userAndOrg);
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
        public async Task<IHttpActionResult> WatchPost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _postService.ToggleWatchAsync(post.Id, userAndOrg, true);
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
        public async Task<IHttpActionResult> UnwatchPost(HidePostViewModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAndOrg = GetUserAndOrganization();
            try
            {
                await _postService.ToggleWatchAsync(post.Id, userAndOrg, false);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            return Ok();
        }
    }
}