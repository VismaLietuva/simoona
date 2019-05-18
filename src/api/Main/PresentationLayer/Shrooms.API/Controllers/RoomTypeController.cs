using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.API.Controllers.WebApi
{
    [Authorize]
    [PermissionAuthorize(AdministrationPermissions.RoomType)]
    public class RoomTypeController : AbstractWebApiController<RoomType, RoomTypeViewModel, RoomTypePostViewModel>
    {
        public RoomTypeController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork, defaultOrderByProperty: "Name")
        {
        }

        #region CRUD

        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override HttpResponseMessage Post([FromBody] RoomTypePostViewModel crudViewModel)
        {
            if (crudViewModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var foundRoomType = GetByIdOrName(crudViewModel.Name, crudViewModel.Id);

            if (foundRoomType != null)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict, new[] { Resources.Models.RoomType.RoomType.RoomTypePutError1 });
            }

            var roomType = _mapper.Map<RoomTypePostViewModel, RoomType>(crudViewModel);

            Repository.Insert(roomType);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override HttpResponseMessage Put([FromBody] RoomTypePostViewModel crudViewModel)
        {
            var roomTypeModel = GetByIdOrName(crudViewModel.Name, crudViewModel.Id, "Rooms");

            if (roomTypeModel == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.RoomType.RoomType.RoomTypePostError1 });
            }

            _mapper.Map(crudViewModel, roomTypeModel);

            Repository.Update(roomTypeModel);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override HttpResponseMessage Delete(int id)
        {
            var roomType = Repository.Get(maxResults: 1, filter: r => r.Id.Equals(id), includeProperties: "Rooms").FirstOrDefault();

            if (roomType == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (roomType.Rooms != null)
            {
                foreach (Room r in roomType.Rooms)
                {
                    r.RoomType = null;
                    r.RoomTypeId = null;
                }
            }

            Repository.DeleteById(roomType.Id);
            UnitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public override PagedViewModel<RoomTypeViewModel> GetPaged(string includeProperties = null, int page = 1, int pageSize = ConstWebApi.DefaultPageSize,
            string sort = null, string dir = "", string s = "")
        {
            return GetFilteredPaged(includeProperties, page, pageSize, sort, dir, p => p.Name.Contains(s));
        }

        #endregion CRUD

        private RoomType GetByIdOrName(string name, int id = -1, string includeProperties = "")
        {
            var test = Repository.Get();
            var res = Repository.Get(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || f.Id.Equals(id), 1, includeProperties: includeProperties).FirstOrDefault();

            return res;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.RoomType)]
        public IEnumerable<RoomTypeViewModel> GetByFloor(int floorId, string includeProperties = "")
        {
            var model = Repository.Get(t => t.Rooms.Any(r => r.FloorId == floorId), includeProperties: includeProperties);
            return _mapper.Map<IEnumerable<RoomType>, IEnumerable<RoomTypeViewModel>>(model);
        }
    }
}