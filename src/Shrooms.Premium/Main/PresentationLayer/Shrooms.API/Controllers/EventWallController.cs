using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using PagedList;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.WebViewModels.Models.Wall;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.Controllers
{
    [Authorize]
    [RoutePrefix("EventWall")]
    public class EventWallController : BaseController
    {
        private readonly IWallService _wallService;
        private readonly IMapper _mapper;

        public EventWallController(IWallService wallService, IMapper mapper)
        {
            _wallService = wallService;
            _mapper = mapper;
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
        [PermissionAuthorize(Permission = BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> GetWall(int wallId)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var userAndOrg = GetUserAndOrganization();

                var wall = await _wallService.GetWallDetails(wallId, userAndOrg);

                if (wall.Type != WallType.Events)
                {
                    return Forbidden();
                }

                var mappedWall = _mapper.Map<WallDto, WallListViewModel>(wall);
                return Ok(mappedWall);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Posts")]
        [PermissionAuthorize(Permission = BasicPermissions.EventWall)]
        public async Task<IHttpActionResult> GetPagedWall(int wallId, int page = 1)
        {
            if (wallId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var userAndOrg = GetUserAndOrganization();

                if (!await IsEventWall(wallId, userAndOrg))
                {
                    return Forbidden();
                }

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

        private async Task<bool> IsEventWall(int wallId, UserAndOrganizationDTO userAndOrg = null)
        {
            if (userAndOrg == null)
            {
                userAndOrg = GetUserAndOrganization();
            }

            var wall = await _wallService.GetWall(wallId, userAndOrg);

            return wall.Type == WallType.Events;
        }
    }
}