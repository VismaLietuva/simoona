using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Filters;
using Shrooms.API.Hubs;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.WebViewModels.Models.Notifications;
using Shrooms.WebViewModels.Models.User;
using Shrooms.WebViewModels.Models.Wall;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.API.Controllers.Wall
{
    [Authorize]
    [RoutePrefix("Wall")]
    public class WallController : BaseController
    {
        private readonly IWallService _wallService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public WallController(IMapper mapper, IWallService wallService, INotificationService notificationService)
        {
            _wallService = wallService;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> GetWallList(WallsListFilter filter)
        {
            var wallList = await _wallService.GetWallsList(GetUserAndOrganization(), filter);
            var mappedWallList = _mapper.Map<IEnumerable<WallDto>, IEnumerable<WallListViewModel>>(wallList);
            return Ok(mappedWallList);
        }

        /// <summary>
        ///     Returns wall details
        /// </summary>
        /// <param name="wallId">Wall id</param>
        /// <response code="200">Wall details</response>
        /// <response code="400">Validation response</response>
        /// <returns>Wall details in HTTP response</returns>
        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> GetWall(int wallId)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            try
            {
                UserAndOrganizationDTO userInfo = GetUserAndOrganization();

                var wall = await _wallService.WallDetails(wallId, userInfo);
                var mappedWall = _mapper.Map<WallDto, WallListViewModel>(wall);
                return Ok(mappedWall);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        /// <summary>
        ///     Become wall member
        /// </summary>
        /// <param name="wallId">Wall id</param>
        /// <param name="attendeeId">UserID that should be added to wall</param>
        /// <response code="200">Successfully joined</response>
        /// <response code="400">Validation response</response>
        /// <returns>HTTP Ok</returns>
        [HttpPut]
        [Route("Follow")]
        [PermissionAuthorize(Permission = BasicPermissions.Wall)]
        public IHttpActionResult JoinWall(int wallId, string attendeeId = null)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            var userOrg = GetUserAndOrganization();
            if (string.IsNullOrEmpty(attendeeId))
            {
                attendeeId = userOrg.UserId;
            }

            try
            {
                var userDto = _wallService.JoinLeaveWall(wallId, attendeeId, userOrg.UserId, userOrg.OrganizationId, false);
                var result = _mapper.Map<ApplicationUserMinimalViewModelDto, ApplicationUserMinimalViewModel>(userDto);

                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }

        /// <summary>
        ///     Return wall members list
        /// </summary>
        /// <param name="wallId">Wall id</param>
        /// <response code="200">List of wall members</response>
        /// <response code="400">Validation response</response>
        /// <returns>List of wall members wrapped in HTTP response</returns>
        [HttpGet]
        [Route("Members")]
        [PermissionAuthorize(Permission = BasicPermissions.Wall)]
        public async Task<IHttpActionResult> GetWallMembers(int wallId)
        {
            try
            {
                var wallMembersDto = await _wallService.GetWallMembers(wallId, GetUserAndOrganization());
                var result = _mapper.Map<IEnumerable<WallMemberDto>, IEnumerable<WallMemberViewModel>>(wallMembersDto);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Posts")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> GetPagedWall(int wallId, int page = 1)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var userAndOrg = GetUserAndOrganization();
                var wallPosts = await _wallService.GetWallPosts(page, ConstWebApi.DefaultPageSize, userAndOrg, wallId);

                var mappedPosts = _mapper.Map<IEnumerable<WallPostViewModel>>(wallPosts);
                var pagedViewModel = new PagedWallViewModel<WallPostViewModel>
                {
                    PagedList = mappedPosts.ToPagedList(1, ConstWebApi.DefaultPageSize),
                    PageSize = ConstWebApi.DefaultPageSize
                };
                return Ok(pagedViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("AllPosts")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> GetAllPagedWall(int page = 1, int wallsType = 1) // wallType = 1 "My walls"   wallType = 2 "All walls"
        {
            var userAndOrg = GetUserAndOrganization();
            var wallPosts = await _wallService.GetAllPosts(page, ConstWebApi.DefaultPageSize, userAndOrg, wallsType);

            var mappedPosts = _mapper.Map<IEnumerable<WallPostViewModel>>(wallPosts);
            var pagedViewModel = new PagedWallViewModel<WallPostViewModel>
            {
                PagedList = mappedPosts.ToPagedList(1, ConstWebApi.DefaultPageSize),
                PageSize = ConstWebApi.DefaultPageSize
            };
            return Ok(pagedViewModel);
        }

        [HttpGet]
        [Route("Search")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public async Task<IHttpActionResult> SearchWall(string searchString, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userAndOrganization = GetUserAndOrganization();
            var foundWallPosts = await _wallService.SearchWall(searchString, userAndOrganization, page, ConstWebApi.DefaultPageSize);

            var mappedPosts = _mapper.Map<IList<WallPostViewModel>>(foundWallPosts);
            var pagedViewModel = new PagedWallViewModel<WallPostViewModel>
            {
                PagedList = mappedPosts.ToPagedList(1, ConstWebApi.DefaultPageSize),
                PageSize = ConstWebApi.DefaultPageSize
            };
            return Ok(pagedViewModel);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Wall)]
        public async Task<IHttpActionResult> CreateWall(CreateWallViewModel newWall)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var wallDto = _mapper.Map<CreateWallViewModel, CreateWallDto>(newWall);
            SetOrganizationAndUser(wallDto);

            try
            {
                var wallId = await _wallService.CreateNewWall(wallDto);

                var notificationDto = await _notificationService.CreateForWall(GetUserAndOrganization(), wallDto, wallId);

                NotificationHub.SendNotificationToAllUsers(_mapper.Map<NotificationViewModel>(notificationDto), GetUserAndOrganizationHub());

                return Ok(new { Id = wallId });
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = BasicPermissions.Wall)]
        public IHttpActionResult EditWall(UpdateWallViewModel updateWallViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateWallDto = _mapper.Map<UpdateWallViewModel, UpdateWallDto>(updateWallViewModel);
            SetOrganizationAndUser(updateWallDto);

            try
            {
                _wallService.UpdateWall(updateWallDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = BasicPermissions.Wall)]
        public IHttpActionResult DeleteWall(int wallId)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            try
            {
                _wallService.DeleteWall(wallId, GetUserAndOrganization(), EntityModels.Models.Multiwall.WallType.UserCreated);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }
    }
}