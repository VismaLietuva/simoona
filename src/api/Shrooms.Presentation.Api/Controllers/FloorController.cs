using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using MoreLinq;
using PagedList;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class FloorController : AbstractWebApiController<Floor, FloorViewModel, FloorPostViewModel>
    {
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<Organization> _organizationRepository;

        public FloorController(IMapper mapper, IUnitOfWork unitOfWork)
            : base(mapper, unitOfWork)
        {
            _officeRepository = unitOfWork.GetRepository<Office>();
            _applicationUserRepository = unitOfWork.GetRepository<ApplicationUser>();
            _organizationRepository = unitOfWork.GetRepository<Organization>();
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override HttpResponseMessage Post(FloorPostViewModel crudViewModel)
        {
            return base.Post(crudViewModel);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override HttpResponseMessage Put(FloorPostViewModel crudViewModel)
        {
            return base.Put(crudViewModel);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Floor)]
        public FloorViewModel GetByRoom(int roomId)
        {
            var model = _repository.Get(f => f.Rooms.Any(r => r.Id == roomId), 1).FirstOrDefault();
            return _mapper.Map<Floor, FloorViewModel>(model);
        }

        [PermissionAuthorize(Permission = BasicPermissions.Floor)]
        public IEnumerable<FloorViewModel> GetByOffice(int officeId)
        {
            var model = _repository.Get(f => f.OfficeId == officeId, includeProperties: "Picture");
            return _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(model);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public FloorViewPagedModel GetAllFloors(int officeId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string s = "", string sort = "Id", string dir = "")
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";
            s = s ?? string.Empty;

            var floors = _repository.GetPaged(f => (officeId == -1 ? f.OfficeId != -1 : f.OfficeId == officeId) && f.Name.Contains(s),
                                                        orderBy: sortQuery, includeProperties: "Rooms,Rooms.ApplicationUsers");

            var floorId = floors.Where(n => n != null).Select(n => n.Id).FirstOrDefault();

            var floorOrganizationId = _applicationUserRepository
                .Get(n => n.Room != null && n.Room.FloorId == floorId)
                .Select(n => n.OrganizationId).FirstOrDefault();

            var floorsViewModel = _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(floors).ToList();

            var organizationName = _organizationRepository
                .Get(n => n.Id == floorOrganizationId)
                .Select(n => n.ShortName).SingleOrDefault();

            foreach (var floorViewModel in floorsViewModel)
            {
                floorViewModel.OrganizationName = organizationName;
            }

            var pagedList = floorsViewModel.ToPagedList(page, pageSize);

            var floorsViewPagedModel = new FloorViewPagedModel
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pagedList.PageSize
            };

            if (officeId != -1)
            {
                var office = _officeRepository.GetByID(officeId);
                floorsViewPagedModel.Office = _mapper.Map<Office, OfficeViewModel>(office);
            }

            floorsViewPagedModel.PagedList.ForEach(f => f.Rooms.ForEach(r =>
            {
                f.ApplicationUsersCount += r.ApplicationUsersCount;
            }));

            return floorsViewPagedModel;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public FloorViewPagedModel GetPaged(int officeId, int page = 1, int pageSize = WebApiConstants.DefaultPageSize, string s = "", string sort = "Id", string dir = "")
        {
            var sortQuery = string.IsNullOrEmpty(sort) ? null : $"{sort} {dir}";
            s = s ?? string.Empty;

            var floors = _repository.GetPaged(f => (officeId == -1 ? f.OfficeId != -1 : f.OfficeId == officeId) && f.Name.Contains(s),
                                                        orderBy: sortQuery, includeProperties: "Picture,Rooms,Rooms.ApplicationUsers");

            var floorId = floors.Where(n => n != null).Select(n => n.Id).FirstOrDefault();

            var floorOrganizationId = _applicationUserRepository.Get(n => n.Room != null && n.Room.FloorId == floorId).Select(n => n.OrganizationId).FirstOrDefault();

            var floorsViewModel = _mapper.Map<IEnumerable<Floor>, IEnumerable<FloorViewModel>>(floors).ToList();

            var organizationName = _organizationRepository.Get(n => n.Id == floorOrganizationId).Select(n => n.ShortName).SingleOrDefault();

            foreach (var floorViewModel in floorsViewModel)
            {
                floorViewModel.OrganizationName = organizationName;
            }

            var pagedList = floorsViewModel.ToPagedList(page, pageSize);

            var floorsViewPagedModel = new FloorViewPagedModel
            {
                PagedList = pagedList,
                PageCount = pagedList.PageCount,
                ItemCount = pagedList.TotalItemCount,
                PageSize = pagedList.PageSize
            };

            if (officeId != -1)
            {
                var office = _officeRepository.GetByID(officeId);
                floorsViewPagedModel.Office = _mapper.Map<Office, OfficeViewModel>(office);
            }

            floorsViewPagedModel.PagedList.ForEach(f => f.Rooms.ForEach(r =>
            {
                f.ApplicationUsersCount += r.ApplicationUsersCount;
            }));

            return floorsViewPagedModel;
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Floor)]
        public override HttpResponseMessage Delete(int id)
        {
            var floor = _repository.Get(filter: of => of.Id == id, includeProperties: "Rooms,Rooms.ApplicationUsers").FirstOrDefault();

            if (floor == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            floor.Rooms.ForEach(r =>
            {
                r.ApplicationUsers.ForEach(e => e.RoomId = null);
                r.FloorId = null;
            });

            _repository.Delete(floor);
            _unitOfWork.Save();

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}