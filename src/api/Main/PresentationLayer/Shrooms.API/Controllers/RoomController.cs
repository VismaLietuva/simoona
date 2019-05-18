using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Shrooms.API.Filters;
using Shrooms.Authentification;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    public class RoomController : AbstractWebApiController<Room, RoomViewModel, RoomPostViewModel>
    {
        public RoomController(IMapper mapper, IUnitOfWork unitOfWork, ShroomsUserManager userManager, ShroomsRoleManager roleManager)
            : base(mapper, unitOfWork, userManager, roleManager, "Id")
        {
        }

        private void RemoveRoomForUsers(RoomPostViewModel roomViewModel)
        {
            var model = Repository.Get(r => r.Id == roomViewModel.Id, includeProperties: "ApplicationUsers").FirstOrDefault();
            if (model.ApplicationUsers.Any())
            {
                var removedFromRoom = model.ApplicationUsers.Where(applicationUser => roomViewModel.ApplicationUsers.All(u => u.Id != applicationUser.Id)).ToList();
                if (!removedFromRoom.Any())
                {
                    return;
                }

                foreach (var applicationUser in removedFromRoom)
                {
                    var user = UserManager.FindById(applicationUser.Id);
                    if (user != null)
                    {
                        user.RoomId = null;
                        UserManager.Update(user);
                    }
                }
            }
        }

        private void AddRoomsForUsers(RoomPostViewModel roomViewModel)
        {
            foreach (var applicationUser in roomViewModel.ApplicationUsers)
            {
                var user = UserManager.FindById(applicationUser.Id);
                user.RoomId = roomViewModel.Id;
                UserManager.Update(user);
            }
        }

        #region CRUD

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public override HttpResponseMessage Post([FromBody] RoomPostViewModel roomViewModel)
        {
            try
            {
                if (Repository.GetByID(roomViewModel.Id) != null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }

                var model = _mapper.Map<RoomPostViewModel, Room>(roomViewModel);
                Repository.Insert(model);
                UnitOfWork.Save();
                roomViewModel.Id = model.Id;
                if (roomViewModel.ApplicationUsers != null)
                {
                    AddRoomsForUsers(roomViewModel);
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
        public override HttpResponseMessage Put([FromBody] RoomPostViewModel roomViewModel)
        {
            try
            {
                var model = Repository.GetByID(roomViewModel.Id);

                if (model == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                _mapper.Map(roomViewModel, model);

                Repository.Update(model);
                UnitOfWork.Save();

                RemoveRoomForUsers(roomViewModel);
                AddRoomsForUsers(roomViewModel);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpDelete]
        [PermissionAuthorize(Permission = AdministrationPermissions.Room)]
        public override HttpResponseMessage Delete(int id)
        {
            try
            {
                var model = Repository.Get(r => r.Id == id, includeProperties: "ApplicationUsers").FirstOrDefault();
                if (model == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                Repository.Delete(model);
                UnitOfWork.Save();
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
        public IEnumerable<RoomViewModel> GetByFloor(int floorId, string includeProperties = "RoomType", bool includeWorkingRooms = true, bool includeNotWorkingRooms = true)
        {
            var rooms = GetRooms(floorId, includeProperties, includeWorkingRooms, includeNotWorkingRooms);
            return _mapper.Map<IEnumerable<Room>, IEnumerable<RoomViewModel>>(rooms);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        private IQueryable<Room> GetRooms(int floorId, string includeProperties, bool includeWorkingRooms, bool includeNotWorkingRooms)
        {
            var rooms = Repository.Get(r => r.FloorId == floorId
                    && ((includeWorkingRooms && r.RoomTypeId != null && r.RoomType.IsWorkingRoom) || (includeNotWorkingRooms && (r.RoomTypeId == null || !r.RoomType.IsWorkingRoom))),
                    includeProperties: includeProperties);
            return rooms;
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public PagedViewModel<RoomViewModel> GetAllRoomsByFloor(int floorId, string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize,
           string s = "", string sort = null, string dir = "")
        {
            return base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, r => (floorId == -1 || r.FloorId == floorId)
                && (r.Name + r.Number).Contains(s));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override HttpResponseMessage Get(int id, string includeProperties = "")
        {
            return base.Get(id, includeProperties);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override PagedViewModel<RoomViewModel> GetPaged(string includeProperties = null, int page = 1,
            int pageSize = ConstWebApi.DefaultPageSize, string sort = null, string dir = "", string s = "")
        {
            return base.GetPaged(includeProperties, page, pageSize, sort, dir, s);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        protected override PagedViewModel<RoomViewModel> GetFilteredPaged(string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize,
            string sort = null, string dir = "", Expression<Func<Room, bool>> filter = null)
        {
            return base.GetFilteredPaged(includeProperties, page, pageSize, sort, dir, filter);
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Room)]
        public override IEnumerable<RoomViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAll(maxResults, orderBy, includeProperties);
        }
    }
}