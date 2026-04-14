using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Microsoft.EntityFrameworkCore;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [PermissionAuthorize(AdministrationPermissions.RoomType)]
    public class RoomTypeController : AbstractWebApiController<RoomType, RoomTypeViewModel, RoomTypePostViewModel>
    {
        public RoomTypeController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork, "Name")
        {
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override async Task<IActionResult> Post([FromBody] RoomTypePostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return BadRequest();
            }

            var foundRoomType = await GetByIdOrNameAsync(crudViewModel.Name, crudViewModel.Id);

            if (foundRoomType != null)
            {
                return StatusCode(409, new[] { Resources.Models.RoomType.RoomType.RoomTypePutError1 });
            }

            var roomType = _mapper.Map<RoomTypePostViewModel, RoomType>(crudViewModel);

            _repository.Insert(roomType);
            await _unitOfWork.SaveAsync();

            return StatusCode(201);
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override async Task<IActionResult> Put([FromBody] RoomTypePostViewModel crudViewModel)
        {
            var roomTypeModel = await GetByIdOrNameAsync(crudViewModel.Name, crudViewModel.Id, "Rooms");

            if (roomTypeModel == null)
            {
                return StatusCode(404, new[] { Resources.Models.RoomType.RoomType.RoomTypePostError1 });
            }

            _mapper.Map(crudViewModel, roomTypeModel);

            _repository.Update(roomTypeModel);
            await _unitOfWork.SaveAsync();

            return StatusCode(201);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override async Task<IActionResult> Delete(int id)
        {
            var roomType = await _repository.Get(maxResults: 1, filter: r => r.Id.Equals(id), includeProperties: "Rooms").FirstOrDefaultAsync();

            if (roomType == null)
            {
                return NotFound();
            }

            if (roomType.Rooms != null)
            {
                foreach (var r in roomType.Rooms)
                {
                    r.RoomType = null;
                    r.RoomTypeId = null;
                }
            }

            await _repository.DeleteByIdAsync(roomType.Id);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override async Task<PagedViewModel<RoomTypeViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            return await GetFilteredPaged(includeProperties, page, pageSize, sort, dir, p => p.Name.Contains(s));
        }

        private async Task<RoomType> GetByIdOrNameAsync(string name, int id = -1, string includeProperties = "")
        {
            var result = await _repository.Get(f => (name != null && f.Name.ToLower() == name.ToLower()) || f.Id == id, 1, includeProperties: includeProperties).FirstOrDefaultAsync();

            return result;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public async Task<IEnumerable<RoomTypeViewModel>> GetByFloor(int floorId, string includeProperties = "")
        {
            var model = await _repository.Get(t => t.Rooms.Any(r => r.FloorId == floorId), includeProperties: includeProperties).ToListAsync();
            return _mapper.Map<IEnumerable<RoomType>, IEnumerable<RoomTypeViewModel>>(model);
        }
    }
}
