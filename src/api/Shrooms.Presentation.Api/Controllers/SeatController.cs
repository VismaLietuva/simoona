using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Seats;
using Shrooms.Domain.Services.Seats;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class SeatController : BaseController
    {
        private readonly ISeatService _seatService;

        public SeatController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Map)]
        [ProducesResponseType(typeof(SeatBoardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBoard(int floorId, string from = null, string to = null)
        {
            if (floorId <= 0)
            {
                return BadRequest("A floor is required.");
            }

            var userOrg = GetUserAndOrganization();
            var board = await _seatService.GetBoardAsync(new SeatBoardArgsDto
            {
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId,
                FloorId = floorId,
                From = from,
                To = to
            });

            return Ok(board);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        [ProducesResponseType(typeof(IEnumerable<SeatDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRoom(int roomId)
        {
            if (roomId <= 0)
            {
                return BadRequest("A room is required.");
            }

            return Ok(await _seatService.GetByRoomAsync(roomId, GetUserAndOrganization()));
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.Map)]
        [ProducesResponseType(typeof(SeatBookResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Book([FromBody] SeatDayArgsDto args)
        {
            return await GuardedAsync(() =>
            {
                var userOrg = GetUserAndOrganization();
                return _seatService.BookAsync(new SeatDayArgsDto
                {
                    UserId = userOrg.UserId,
                    OrganizationId = userOrg.OrganizationId,
                    SeatId = args.SeatId,
                    Day = args.Day
                });
            });
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.Map)]
        public async Task<IActionResult> GoHome([FromBody] SeatDayArgsDto args)
        {
            try
            {
                await _seatService.GoHomeAsync(args.Day, GetUserAndOrganization());
                return Ok();
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.Map)]
        public async Task<IActionResult> Unrelease([FromBody] SeatDayArgsDto args)
        {
            try
            {
                var userOrg = GetUserAndOrganization();
                await _seatService.UnreleaseAsync(new SeatDayArgsDto
                {
                    UserId = userOrg.UserId,
                    OrganizationId = userOrg.OrganizationId,
                    SeatId = args.SeatId,
                    Day = args.Day
                });

                return Ok();
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        [ProducesResponseType(typeof(SeatDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] SeatSaveArgsDto args)
        {
            return await GuardedAsync(() =>
            {
                var userOrg = GetUserAndOrganization();
                return _seatService.CreateAsync(WithUser(args, userOrg.UserId, userOrg.OrganizationId));
            });
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        [ProducesResponseType(typeof(SeatDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] SeatSaveArgsDto args)
        {
            return await GuardedAsync(() =>
            {
                var userOrg = GetUserAndOrganization();
                return _seatService.UpdateAsync(WithUser(args, userOrg.UserId, userOrg.OrganizationId));
            });
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _seatService.DeleteAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
        }

        private static SeatSaveArgsDto WithUser(SeatSaveArgsDto args, string userId, int organizationId)
        {
            return new SeatSaveArgsDto
            {
                UserId = userId,
                OrganizationId = organizationId,
                Id = args.Id,
                RoomId = args.RoomId,
                Name = args.Name,
                Type = args.Type,
                X = args.X,
                Y = args.Y,
                OwnerId = args.OwnerId
            };
        }

        private async Task<IActionResult> GuardedAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
