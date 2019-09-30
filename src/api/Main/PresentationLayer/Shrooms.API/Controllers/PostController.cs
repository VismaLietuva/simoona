using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.BackgroundWorkers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.Infrastructure.FireAndForget;
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
        private readonly IAsyncRunner _asyncRunner;

        public PostController(IMapper mapper, IWallService wallService, IPostService postService, IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _wallService = wallService;
            _postService = postService;
            _asyncRunner = asyncRunner;
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
        public IHttpActionResult CreatePost(CreateWallPostViewModel wallPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                _postService.ToggleWatch(post.Id, userAndOrg,true);
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