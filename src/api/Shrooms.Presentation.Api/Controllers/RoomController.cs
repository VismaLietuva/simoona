using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using X.PagedList;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class RoomController : AbstractWebApiController<Room, RoomViewModel, RoomPostViewModel>
    {
        public RoomController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager)
            : base(mapper, unitOfWork, userManager, "Id")
        {
        }

        #region CRUD

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public override async Task<HttpResponseMessage> Post([FromBody] RoomPostViewModel roomViewModel)
        {
            try
            {
                if (await _repository.GetByIdAsync(roomViewModel.Id) != null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }

                var model = _mapper.Map<RoomPostViewModel, Room>(roomViewModel);
                _repository.Insert(model);

                await _unitOfWork.SaveAsync();
                roomViewModel.Id = model.Id;

                if (roomViewModel.ApplicationUsers != null)
                {
                    await AddRoomsForUsersAsync(roomViewModel);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public override async Task<HttpResponseMessage> Put([FromBody] RoomPostViewModel roomViewModel)
        {
            try
            {
                var model = await _repository.GetByIdAsync(roomViewModel.Id);

                if (model == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                _mapper.Map(roomViewModel, model);

                _repository.Update(model);
                await _unitOfWork.SaveAsync();

                await RemoveRoomForUsersAsync(roomViewModel);
                await AddRoomsForUsersAsync(roomViewModel);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public override async Task<HttpResponseMessage> Delete(int id)
        {
            try
            {
                var model = await _repository.Get(r => r.Id == id, includeProperties: "ApplicationUsers").FirstOrDefaultAsync();
                if (model == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                _repository.Delete(model);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion CRUD

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public async Task<IEnumerable<RoomViewModel>> GetByFloor(int floorId, string includeProperties = "RoomType", bool includeWorkingRooms = true, bool includeNotWorkingRooms = true)
        {
            var rooms = await GetRooms(floorId, includeProperties, includeWorkingRooms, includeNotWorkingRooms);
            return _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        private async Task<IList<Room>> GetRooms(int floorId, string includeProperties, bool includeWorkingRooms, bool includeNotWorkingRooms)
        {
            var rooms = _repository.Get(r => r.FloorId == floorId
                                             && ((includeWorkingRooms && r.RoomTypeId != null && r.RoomType.IsWorkingRoom) ||
                                                 (includeNotWorkingRooms && (r.RoomTypeId == null || !r.RoomType.IsWorkingRoom))),
                includeProperties: includeProperties);
            return await rooms.ToListAsync();
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public async Task<PagedViewModel<RoomViewModel>> GetAllRoomsByFloor(int floorId,
            string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string s = "",
            string sort = null,
            string dir = "")
        {
            return await base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, r => (floorId == -1 || r.FloorId == floorId)
                                                                                                       && (r.Name + r.Number).Contains(s));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override async Task<HttpResponseMessage> Get(int id, string includeProperties = "")
        {
            return await base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override async Task<PagedViewModel<RoomViewModel>> GetPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            string s = "")
        {
            return await base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        protected override async Task<PagedViewModel<RoomViewModel>> GetFilteredPaged(string includeProperties = null,
            int page = 1,
            int pageSize = WebApiConstants.DefaultPageSize,
            string sort = null,
            string dir = "",
            Expression<Func<Room, bool>> filter = null)
        {
            return await base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override async Task<IEnumerable<RoomViewModel>> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return await base.GetAll(maxResults, orderBy, includeProperties);
        }

        private async Task RemoveRoomForUsersAsync(RoomPostViewModel roomViewModel)
        {
            var model = await _repository.Get(r => r.Id == roomViewModel.Id, includeProperties: "ApplicationUsers").FirstOrDefaultAsync();
            if (model == null || !model.ApplicationUsers.Any())
            {
                return;
            }

            var removedFromRoom = await model.ApplicationUsers.Where(applicationUser => roomViewModel.ApplicationUsers.All(u => u.Id != applicationUser.Id)).ToListAsync();
            if (!removedFromRoom.Any())
            {
                return;
            }

            foreach (var applicationUser in removedFromRoom)
            {
                var user = await _userManager.FindByIdAsync(applicationUser.Id);
                if (user != null)
                {
                    user.RoomId = null;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        private async Task AddRoomsForUsersAsync(RoomPostViewModel roomViewModel)
        {
            foreach (var applicationUser in roomViewModel.ApplicationUsers)
            {
                var user = await _userManager.FindByIdAsync(applicationUser.Id);
                user.RoomId = roomViewModel.Id;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
